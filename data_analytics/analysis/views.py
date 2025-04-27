from google.cloud import bigquery
from django.http import JsonResponse, Http404, HttpResponseServerError
from dotenv import load_dotenv
from google.api_core.exceptions import GoogleAPIError
import os
import re
import requests
from dotenv import load_dotenv
from django.http import JsonResponse
from django.views.decorators.http import require_GET
from pymongo import MongoClient
from bson import ObjectId


# ——————————————————————————————————————————————————————————
# Завантажуємо .env і ініціалізуємо клієнти
# ——————————————————————————————————————————————————————————
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
load_dotenv(os.path.join(BASE_DIR, '.env'))
MONGO_URI       = os.getenv("MONGO_URI")
MONGO_DB        = os.getenv("MONGO_DB")
GOOGLE_API_KEY  = os.getenv("API")  # ваш Google API key
CX              = os.getenv("KEY")  # Custom Search Engine ID
BQ_PROJECT_ID  = os.getenv("BQ_PROJECT_ID")
BQ_DATASET     = os.getenv("BQ_DATASET")

mongo_client = MongoClient(MONGO_URI)
db           = mongo_client[MONGO_DB]

def search_properties_with_google(biz):
    """
    Шукає пропозиції нерухомості через Google CSE
    і повертає список dict з ключами:
      type, location, area_sqm, link, price
    """
    query = f"{biz['description']} оренда купівля {biz['location']['city']}"
    params = {
        "key": GOOGLE_API_KEY,
        "cx":  CX,
        "q":   query,
        "num": 10
    }
    resp = requests.get("https://www.googleapis.com/customsearch/v1", params=params, timeout=5)
    resp.raise_for_status()
    data = resp.json()
    print("CSE RESPONSE:", data)   # or use logging.debug

    listings = []
    for item in data.get("items", []):
        snippet = item.get("snippet", "").lower()
        t = "rent" if "оренда" in snippet or "rent" in snippet else "buy"
        m = re.search(r"([\d\s,]+)\s*грн", snippet)
        price = int(m.group(1).replace(" ", "").replace(",", "")) if m else None

        listings.append({
            "type":     t,
            "location": biz["location"]["city"],
            "area_sqm": biz.get("area_sqm"),
            "link":     item.get("link"),
            "price":    price
        })

    return listings

@require_GET
def google_search_test(request, biz_id):
    """
    GET /api/business/<biz_id>/google-search/
    Протестувати тільки Google CSE пошук по бізнесу.
    """
    try:
        biz = db.BusinessData.find_one({"_id": ObjectId(biz_id)})
    except Exception:
        return JsonResponse({"error": "Invalid business ID"}, status=400)
    if not biz:
        return JsonResponse({"error": "Business not found"}, status=404)

    try:
        listings = search_properties_with_google(biz)
    except Exception as e:
        return JsonResponse({"error": str(e)}, status=500)

    return JsonResponse(listings, safe=False)

@require_GET
def property_recommendations(request, biz_id):
    """
    GET /api/business/<biz_id>/properties/
    — повертає список пропозицій нерухомості для бізнесу biz_id
    """
    # 1) Дістаємо бізнес із MongoDB
    try:
        oid = ObjectId(biz_id)
    except Exception:
        return JsonResponse({"error": "Invalid business ID"}, status=400)

    biz = db.BusinessData.find_one({"_id": oid})
    if not biz:
        return JsonResponse({"error": "Business not found"}, status=404)

    # 2) Викликаємо Google CSE
    try:
        listings = search_properties_with_google(biz)
    except Exception as e:
        return JsonResponse({"error": str(e)}, status=500)

    # 3) Повертаємо чистий JSON-масив
    return JsonResponse(listings, safe=False)



@require_GET
def regions_losses(request):
    project = os.getenv("BQ_PROJECT_ID")
    dataset = os.getenv("BQ_DATASET")
    if not project or not dataset:
        return HttpResponseServerError("BQ_PROJECT_ID або BQ_DATASET не налаштовані")

    client = bigquery.Client(project=project)
    sql = f"""
    WITH
      region_list AS (
        SELECT region
        FROM UNNEST([
          "Vinnytsia Oblast","Volyn Oblast","Dnipropetrovsk Oblast","Donetsk Oblast",
          "Zhytomyr Oblast","Zakarpattia Oblast","Zaporizhzhia Oblast","Ivano-Frankivsk Oblast",
          "Kyiv Oblast","Kirovohrad Oblast","Luhansk Oblast","Lviv Oblast",
          "Mykolaiv Oblast","Odesa Oblast","Poltava Oblast","Rivne Oblast",
          "Sumy Oblast","Ternopil Oblast","Kharkiv Oblast","Kherson Oblast",
          "Khmelnytskyi Oblast","Cherkasy Oblast","Chernivtsi Oblast","Chernihiv Oblast"
        ]) AS region
      ),
      latest_losses AS (
        SELECT business_id, total_loss,
               ROW_NUMBER() OVER (
                 PARTITION BY business_id
                 ORDER BY loss_dt DESC
               ) AS rn
        FROM `{project}.{dataset}.fact_business_loss`
      ),
      agg AS (
        SELECT
          db.region,
          SUM(ll.total_loss) AS total_loss
        FROM `{project}.{dataset}.dim_business` AS db
        JOIN latest_losses AS ll
          ON db.business_id = ll.business_id
         AND ll.rn = 1
        GROUP BY db.region
      )
    SELECT
      rl.region,
      COALESCE(a.total_loss, 0) AS total_loss
    FROM region_list rl
    LEFT JOIN agg a
      ON rl.region = a.region
    ORDER BY rl.region
    """

    try:
        query_job = client.query(sql)
        rows = query_job.result()
    except GoogleAPIError as e:
        return HttpResponseServerError(f"BigQuery Error: {e}")

    data = [{"region": row.region, "total_loss": row.total_loss} for row in rows]
    return JsonResponse(data, safe=False)


@require_GET
def total_losses(request):
    """
    Повертає одне число: суму total_loss по кожному бізнесу, беручи лише останній запис (max loss_dt).
    """
    project = os.getenv("BQ_PROJECT_ID")
    dataset = os.getenv("BQ_DATASET")
    if not project or not dataset:
        return HttpResponseServerError("BQ_PROJECT_ID або BQ_DATASET не налаштовані")

    client = bigquery.Client(project=project)
    sql = f"""
    WITH latest_losses AS (
      SELECT total_loss,
             ROW_NUMBER() OVER (
               PARTITION BY business_id
               ORDER BY loss_dt DESC
             ) AS rn
      FROM `{project}.{dataset}.fact_business_loss`
    )
    SELECT COALESCE(SUM(total_loss), 0) AS total_sum
    FROM latest_losses
    WHERE rn = 1
    """

    try:
        job = client.query(sql)
        row = next(job.result(), None)
        total = row.total_sum if row else 0
    except GoogleAPIError as e:
        return HttpResponseServerError(f"BigQuery Error: {e}")

    # Повертаємо просто число у форматі JSON-літералу
    return JsonResponse(total, safe=False)



