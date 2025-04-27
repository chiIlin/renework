import os
import datetime
import random
from dotenv import load_dotenv
from pymongo import MongoClient
from bson import ObjectId

# ——————————————————————————————————————————————————————————
# 1) Завантажимо налаштування з .env
# ——————————————————————————————————————————————————————————
load_dotenv()
MONGO_URI = os.getenv("MONGO_URI")
DB_NAME   = os.getenv("MONGO_DB")
if not MONGO_URI or not DB_NAME:
    raise RuntimeError("Не знайдено MONGO_URI або MONGO_DB у .env")

# ——————————————————————————————————————————————————————————
# 2) Підключаємося до MongoDB
# ——————————————————————————————————————————————————————————
client = MongoClient(MONGO_URI)
db = client[DB_NAME]
users_col    = db["Users"]
business_col = db["BusinessData"]

# ——————————————————————————————————————————————————————————
# 3) Видаляємо всіх користувачів, окрім одного
# ——————————————————————————————————————————————————————————
keep_id = ObjectId("680d0b1a5665b8dd850b212e")
users_col.delete_many({"_id": {"$ne": keep_id}})

# ——————————————————————————————————————————————————————————
# 4) Очищаємо бізнес-колекцію
# ——————————————————————————————————————————————————————————
business_col.delete_many({})

# ——————————————————————————————————————————————————————————
# 5) Створюємо 10 бізнес-документів з полем downtimeStart
# ——————————————————————————————————————————————————————————
cities = [
    "Kyiv","Lviv","Odesa","Kharkiv",
    "Dnipro","Vinnytsia","Chernihiv","Ivano-Frankivsk"
]
city_to_region = {
    "Kyiv": "Kyiv Oblast",
    "Lviv": "Lviv Oblast",
    "Odesa": "Odesa Oblast",
    "Kharkiv": "Kharkiv Oblast",
    "Dnipro": "Dnipropetrovsk Oblast",
    "Vinnytsia": "Vinnytsia Oblast",
    "Chernihiv": "Chernihiv Oblast",
    "Ivano-Frankivsk": "Ivano-Frankivsk Oblast"
}
sample_descriptions = [
    "Well-located space with high foot traffic.",
    "Ideal for retail or office, near public transport.",
    "Spacious premises with modern amenities.",
    "Recently renovated, ready for occupancy.",
    "Affordable, suitable for startups."
]

business_docs = []
for _ in range(10):
    bid = ObjectId()
    city = random.choice(cities)
    # times
    created_at = datetime.datetime.utcnow()
    # downtime duration in months, to compute total loss
    downtime_months = random.randint(1, 12)
    # start date = created_at minus approximate downtime days
    downtime_start = created_at - datetime.timedelta(days=downtime_months * 30)
    # financials
    monthly_rev = round(random.uniform(20000, 200000), 2)
    budget      = round(random.uniform(5000, 50000), 2)
    indirect    = monthly_rev * downtime_months
    direct      = budget
    total_loss  = round(direct + indirect, 2)

    business_docs.append({
        "_id": bid,
        "location": {
            "city":   city,
            "region": city_to_region[city],
            "address": f"{random.randint(1,200)} {random.choice(['Main','Freedom','Central','Union'])} St."
        },
        "area_sqm":        round(random.uniform(20,300), 1),
        "monthlyRevenue":  monthly_rev,
        "budget":          budget,
        "description":     random.choice(sample_descriptions),
        "downtimeStart":   downtime_start,    # нове поле
        "createdAt":       created_at,
        "totalLosses":     total_loss
    })

inserted_ids = business_col.insert_many(business_docs).inserted_ids

# ——————————————————————————————————————————————————————————
# 6) Прив’язуємо кожен бізнес лише до одного користувача
# ——————————————————————————————————————————————————————————
available = inserted_ids.copy()
keep_biz = available.pop(0)
users_col.update_one(
    {"_id": keep_id},
    {
        "$set":   {"businessIds": [keep_biz]},
        "$unset": {"businessId": ""}
    }
)

# ——————————————————————————————————————————————————————————
# 7) Створюємо 10 нових користувачів, кожному по унікальному бізнесу
# ——————————————————————————————————————————————————————————
sample_skills  = ["management","sales","marketing","python","data_analysis","design"]
sample_courses = ["course_python","course_ml_basics","course_web_dev","course_data_viz"]

new_users = []
for i in range(1, 11):
    uid = ObjectId()
    biz_list = [available.pop(0)] if available else []
    new_users.append({
        "_id": uid,
        "username":       f"user{i}",
        "firstName":      f"First{i}",
        "surname":        f"Surname{i}",
        "hashedPassword": "$2a$11$examplehashxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        "role":           "User",
        "email":          f"user{i}@example.com",
        "skills":         random.sample(sample_skills, k=random.randint(0,len(sample_skills))),
        "description":    f"Test user #{i}",
        "appliedCourses": random.sample(sample_courses, k=random.randint(0,len(sample_courses))),
        "companyName":    None,
        "businessIds":    biz_list,
        "createdAt":      datetime.datetime.utcnow()
    })

users_col.insert_many(new_users)

print(f"✅ Користувач {keep_id} отримав бізнес {keep_biz}, створено {len(new_users)} нових юзерів і {len(business_docs)} бізнесів із датою початку даунтайму")
