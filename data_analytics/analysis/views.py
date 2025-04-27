from google.cloud import bigquery
import os
import json
from django.http import JsonResponse, Http404, HttpResponseServerError
from django.views.decorators.http import require_GET
from pymongo import MongoClient
from bson import ObjectId
from openai import OpenAI
from dotenv import load_dotenv
from google.api_core.exceptions import GoogleAPIError

load_dotenv()


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




@require_GET
def property_recommendations(request, biz_id):
    # ініціалізуємо клієнт OpenAI v1.x
    client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

    # MongoDB
    mongo = MongoClient(os.getenv("MONGO_URI"))[os.getenv("MONGO_DB")]
    # 1) Дістаємо бізнес
    try:
        biz = mongo.BusinessData.find_one({"_id": ObjectId(biz_id)})
    except Exception:
        return JsonResponse({"error": "Invalid business ID"}, status=400)
    if not biz:
        return JsonResponse({"error": "Business not found"}, status=404)

    # витягуємо дані
    description  = biz.get("description", "")
    budget       = biz.get("budget", 0)
    total_losses = biz.get("totalLosses", 0)
    net_budget   = round(budget - total_losses, 2)

    # 2) Формуємо промт
    system_msg = (
        "Ти — асистент із підбору комерційної нерухомості для відновлення малого бізнесу. "
        "Отримуєш опис бізнесу, початковий бюджет і збитки після зищення попереднього місця цього бізнесу, "
        "маєш повернути JSON-масив пропозицій з полями: "
        '`type` ("rent" або "buy"), '
        '`location`, `area_sqm`, `link`, `price`. '
        "Бюджет після збитків = net_budget."
    )
    user_payload = {
        "description":      description,
        "original_budget":  budget,
        "total_losses":     total_losses,
        "net_budget":       net_budget
    }

    # 3) Викликаємо новий метод v1.x
    try:
        resp = client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system",  "content": system_msg},
                {"role": "user",    "content": json.dumps(user_payload, ensure_ascii=False)}
            ],
            temperature=0.7,
        )
        # витягуємо JSON з відповіді
        text = resp.choices[0].message.content.strip()
        suggestions = json.loads(text)
    except Exception as e:
        return HttpResponseServerError(f"OpenAI error: {e}")

    return JsonResponse(suggestions, safe=False)