# data_analytics/Dockerfile

FROM python:3.11-slim
WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

ENV DJANGO_SETTINGS_MODULE=data_analytics.settings
EXPOSE 8000

CMD ["gunicorn", "data_analytics.wsgi:application", "--bind", "0.0.0.0:8000"]
