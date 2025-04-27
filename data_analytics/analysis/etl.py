import os
import datetime
from dateutil.relativedelta import relativedelta
from dotenv import load_dotenv
from pymongo import MongoClient
from google.cloud import bigquery

# ——————————————————————————————————————————————————————————
# 1) Читаємо .env
# ——————————————————————————————————————————————————————————
load_dotenv()
MONGO_URI     = os.getenv("MONGO_URI")
MONGO_DB      = os.getenv("MONGO_DB")
BQ_PROJECT_ID = os.getenv("BQ_PROJECT_ID")
BQ_DATASET    = os.getenv("BQ_DATASET")

if not all([MONGO_URI, MONGO_DB, BQ_PROJECT_ID, BQ_DATASET]):
    raise RuntimeError("Не знайдено одну з MONGO_URI, MONGO_DB, BQ_PROJECT_ID або BQ_DATASET")

# ——————————————————————————————————————————————————————————
# 2) Підключення
# ——————————————————————————————————————————————————————————
mongo = MongoClient(MONGO_URI)[MONGO_DB]
bq   = bigquery.Client(project=BQ_PROJECT_ID)

def load_table(table_name: str, rows: list):
    if not rows:
        print(f"⚠️  {table_name}: немає рядків, пропускаємо")
        return
    table_ref = f"{BQ_PROJECT_ID}.{BQ_DATASET}.{table_name}"
    job_config = bigquery.LoadJobConfig(
        write_disposition=bigquery.WriteDisposition.WRITE_TRUNCATE,
        autodetect=True,
        source_format=bigquery.SourceFormat.NEWLINE_DELIMITED_JSON
    )
    def conv(o):
        if isinstance(o, (datetime.date, datetime.datetime)):
            return o.isoformat()
        return o
    json_rows = [{k: conv(v) for k, v in row.items()} for row in rows]
    job = bq.load_table_from_json(json_rows, table_ref, job_config=job_config)
    job.result()
    print(f"✅ {table_name}: завантажено {len(json_rows)} рядків")

# ——————————————————————————————————————————————————————————
# 3) dim_user
# ——————————————————————————————————————————————————————————
users = list(mongo.Users.find())
load_table("dim_user", [
    {
        "user_id":    str(u["_id"]),
        "username":   u.get("username"),
        "role":       u.get("role"),
        "email":      u.get("email"),
        "first_name": u.get("firstName"),
        "surname":    u.get("surname"),
        "created_at": u.get("createdAt")
    }
    for u in users
])

# ——————————————————————————————————————————————————————————
# 4) dim_business — додаємо downtime_start, прибираємо downtime_months
# ——————————————————————————————————————————————————————————
bizs = list(mongo.BusinessData.find())
load_table("dim_business", [
    {
        "business_id":    str(b["_id"]),
        "city":           b["location"].get("city"),
        "region":         b["location"].get("region"),
        "address":        b["location"].get("address"),
        "area_sqm":       b.get("area_sqm"),
        "budget":         b.get("budget"),
        "description":    b.get("description"),
        "downtime_start": b.get("downtimeStart"),
        "created_at":     b.get("createdAt")
    }
    for b in bizs
])

# ——————————————————————————————————————————————————————————
# 5) bridge_user_business
# ——————————————————————————————————————————————————————————
bridge = []
for u in users:
    for bid in u.get("businessIds", []):
        b = mongo.BusinessData.find_one({"_id": bid})
        bridge.append({
            "user_id":     str(u["_id"]),
            "business_id": str(bid),
            "assigned_at": b.get("createdAt") if b else None
        })
load_table("bridge_user_business", bridge)

# ——————————————————————————————————————————————————————————
# 6) fact_business_loss — використовуємо downtime_start для обчислення місяців простою
# ——————————————————————————————————————————————————————————
loss = []
for b in bizs:
    loss_dt = b.get("createdAt")
    dt_start = b.get("downtimeStart")
    # приблизно вираховуємо місяці простою
    downtime_months = None
    if isinstance(dt_start, datetime.datetime) and isinstance(loss_dt, datetime.datetime):
        delta = relativedelta(loss_dt, dt_start)
        downtime_months = delta.years * 12 + delta.months + delta.days / 30
    loss.append({
        "business_id":     str(b["_id"]),
        "loss_dt":         loss_dt.date() if loss_dt else None,
        "monthly_revenue": b.get("monthlyRevenue"),
        "budget":          b.get("budget"),
        "downtime_start":  dt_start,
        "downtime_months": round(downtime_months, 2) if downtime_months is not None else None,
        "direct_loss":     b.get("budget"),
        "indirect_loss":   round((b.get("monthlyRevenue", 0) * downtime_months), 2) if downtime_months else 0,
        "total_loss":      b.get("totalLosses"),
        "insert_ts":       datetime.datetime.utcnow()
    })
load_table("fact_business_loss", loss)

# ——————————————————————————————————————————————————————————
# 7) dim_course
# ——————————————————————————————————————————————————————————
courses = list(mongo.Courses.find())
load_table("dim_course", [
    {
        "course_id":   str(c["_id"]),
        "title":       c.get("title"),
        "description": c.get("description"),
        "tags":        c.get("tags"),
        "duration":    c.get("duration"),
        "company":     c.get("company"),
        "link":        c.get("link"),
        "status":      c.get("status"),
        "created_at":  c.get("timestamp")
    }
    for c in courses
])

# ——————————————————————————————————————————————————————————
# 8) fact_application
# ——————————————————————————————————————————————————————————
apps = list(mongo.Applications.find())
load_table("fact_application", [
    {
        "application_id": str(a["_id"]),
        "user_id":        str(a.get("userId")),
        "course_id":      str(a.get("courseId")),
        "status":         a.get("status"),
        "application_dt": a.get("createdAt").date() if a.get("createdAt") else None,
        "insert_ts":      datetime.datetime.utcnow()
    }
    for a in apps
])

# ——————————————————————————————————————————————————————————
# 9) fact_course_review
# ——————————————————————————————————————————————————————————
reviews = list(mongo.CourseReview.find())
load_table("fact_course_review", [
    {
        "review_id": str(r["_id"]),
        "user_id":   str(r.get("userId")),
        "course_id": str(r.get("courseId")),
        "rating":    r.get("rating"),
        "comment":   r.get("comment"),
        "company":   r.get("company"),
        "review_dt": r.get("createdAt").date() if r.get("createdAt") else None,
        "insert_ts": datetime.datetime.utcnow()
    }
    for r in reviews
])

# ——————————————————————————————————————————————————————————
# 10) fact_applied_course
# ——————————————————————————————————————————————————————————
applied = list(mongo.AppliedCourse.find())
load_table("fact_applied_course", [
    {
        "applied_id": str(a["_id"]),
        "user_id":    None,
        "course_id":  str(a.get("courseId")),
        "progress":   a.get("progress"),
        "rating":     a.get("rating"),
        "duration":   a.get("duration"),
        "applied_dt": a.get("createdAt").date() if a.get("createdAt") else None,
        "insert_ts":  datetime.datetime.utcnow()
    }
    for a in applied
])

print("✅ ETL завершено!")
