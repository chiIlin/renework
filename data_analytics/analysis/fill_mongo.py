import os
import datetime
import random
from dotenv import load_dotenv
from pymongo import MongoClient
from bson import ObjectId

# ——————————————————————————————————————————————————————————
# 1) Завантажуємо налаштування з .env
# ——————————————————————————————————————————————————————————
load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
DB_NAME   = os.getenv("MONGO_DB")

if not MONGO_URI or not DB_NAME:
    raise RuntimeError("Не знайдено MONGO_URI або MONGO_DB у .env")

# ——————————————————————————————————————————————————————————
# 2) Підключаємося та очищаємо БД
# ——————————————————————————————————————————————————————————
client = MongoClient(MONGO_URI)
client.drop_database(DB_NAME)
db = client[DB_NAME]

users_col    = db["Users"]
business_col = db["BusinessData"]

# ——————————————————————————————————————————————————————————
# 3) Генеруємо 10 бізнес-документів (без посилань на юзерів)
# ——————————————————————————————————————————————————————————
cities = ["Kyiv", "Lviv", "Odesa", "Kharkiv", "Dnipro", "Vinnytsia", "Chernihiv", "Ivano-Frankivsk"]

business_docs = []
for _ in range(10):
    bid = ObjectId()
    doc = {
        "_id": bid,
        "location": {
            "city": random.choice(cities),
            "address": f"{random.randint(1,200)} {random.choice(['Main','Freedom','Central','Union'])} St."
        },
        "area_sqm": round(random.uniform(20.0, 300.0), 1),
        "monthlyRevenue": round(random.uniform(20000.0, 200000.0), 2),
        "budget": round(random.uniform(5000.0, 50000.0), 2),
        "createdAt": datetime.datetime.utcnow()
    }
    business_docs.append(doc)

inserted_business_ids = business_col.insert_many(business_docs).inserted_ids

# ——————————————————————————————————————————————————————————
# 4) Генеруємо 10 користувачів і прив’язуємо кожному по бізнесId
# ——————————————————————————————————————————————————————————
sample_skills  = ["management", "sales", "marketing", "python", "data_analysis", "design"]
sample_courses = ["course_python", "course_ml_basics", "course_web_dev", "course_data_viz"]

user_docs = []
for i in range(1, 11):
    uid = ObjectId()
    username = f"user{i}"
    # вибираємо один із створених бізнесів випадково
    biz_ref = random.choice(inserted_business_ids)
    user = {
        "_id": uid,
        "username": username,
        "firstName": f"First{i}",
        "surname":  f"Surname{i}",
        "hashedPassword": "$2a$11$examplehashxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        "role": "User",
        "email": f"{username}@example.com",
        "skills": random.sample(sample_skills, k=random.randint(0, len(sample_skills))),
        "description": f"Test user #{i}",
        "appliedCourses": random.sample(sample_courses, k=random.randint(0, len(sample_courses))),
        "companyName": None,       # у всіх статусу User — None
        "businessId": biz_ref,
        "createdAt": datetime.datetime.utcnow()
    }
    user_docs.append(user)

users_col.insert_many(user_docs)

print(f"✅ Створено {len(business_docs)} бізнесів і {len(user_docs)} користувачів у БД «{DB_NAME}»")
