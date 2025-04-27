import os
import datetime
import random
import uuid
from dotenv import load_dotenv
from pymongo import MongoClient

# ——————————————————————————————————————————————————————————
# 1) Завантажуємо налаштування з .env
# ——————————————————————————————————————————————————————————
load_dotenv()
MONGO_URI = os.getenv("MONGO_URI")
DB_NAME   = os.getenv("MONGO_DB")
if not MONGO_URI or not DB_NAME:
    raise RuntimeError("Не знайдено MONGO_URI або MONGO_DB у .env")

# ——————————————————————————————————————————————————————————
# 2) Підключаємося до MongoDB і очищаємо колекції
# ——————————————————————————————————————————————————————————
client = MongoClient(MONGO_URI)
db = client[DB_NAME]
for col in ["Courses", "Applications", "CourseReview", "AppliedCourse"]:
    db[col].delete_many({})

# ——————————————————————————————————————————————————————————
# 3) Створюємо 10 курсів у колекції Courses
# ——————————————————————————————————————————————————————————
tags      = ["python", "ml", "data", "web", "devops", "design"]
statuses  = ["draft", "published", "archived"]
companies = ["AcmeCorp", "Globex", "Initech", "Umbrella", "StarkIndustries"]

courses = []
for i in range(1, 11):
    courses.append({
        "title":       f"Course #{i} on {random.choice(tags).title()}",
        "description": f"This is an in-depth course on {random.choice(tags)}.",
        "tags":        random.sample(tags, k=random.randint(1, 3)),
        "duration":    f"{random.randint(1, 12)} weeks",
        "company":     random.choice(companies),
        "link":        f"https://example.com/courses/{uuid.uuid4()}",
        "timestamp":   datetime.datetime.utcnow(),
        "status":      random.choice(statuses)
    })
inserted = db["Courses"].insert_many(courses)
course_ids = inserted.inserted_ids

# ——————————————————————————————————————————————————————————
# 4) Підтягуємо userId з існуючої колекції Users
# ——————————————————————————————————————————————————————————
user_ids = [u["_id"] for u in db["Users"].find({}, {"_id": 1})]
if not user_ids:
    raise RuntimeError("Колекція Users порожня. Спочатку заповніть Users.")

# ——————————————————————————————————————————————————————————
# 5) Створюємо 10 заявок у колекції Applications
# ——————————————————————————————————————————————————————————
app_status = ["pending", "accepted", "rejected"]
applications = []
for _ in range(10):
    applications.append({
        "userId":   random.choice(user_ids),
        "courseId": random.choice(course_ids),
        "status":   random.choice(app_status),
        "letter":   "Dear team, I am very interested in this course.",
        "CV":       f"cv_{uuid.uuid4()}.pdf"
    })
db["Applications"].insert_many(applications)

# ——————————————————————————————————————————————————————————
# 6) Створюємо 10 відгуків у колекції CourseReview
# ——————————————————————————————————————————————————————————
comments = [
    "Great course, learned a lot!",
    "Too basic, expected deeper material.",
    "Excellent instructor and exercises.",
    "Content was outdated in parts.",
    "Well-structured and hands-on."
]
reviews = []
for _ in range(10):
    reviews.append({
        "userId":   random.choice(user_ids),
        "courseId": random.choice(course_ids),
        "comment":  random.choice(comments),
        "rating":   random.randint(1, 5),
        "company":  random.choice(companies)
    })
db["CourseReview"].insert_many(reviews)

# ——————————————————————————————————————————————————————————
# 7) Створюємо 10 записів у колекції AppliedCourse
# ——————————————————————————————————————————————————————————
applied = []
for _ in range(10):
    applied.append({
        "courseId": random.choice(course_ids),
        "progress": round(random.random(), 2),
        "rating":   round(random.uniform(0, 5), 1),
        "duration": f"{random.randint(1, 12)} weeks",
        "timestamp": datetime.datetime.utcnow()
    })
db["AppliedCourse"].insert_many(applied)

print("✅ Колекції Courses, Applications, CourseReview та AppliedCourse оновлено!")
