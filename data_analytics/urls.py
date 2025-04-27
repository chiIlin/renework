# data_analytics/urls.py

from django.urls import path, include

urlpatterns = [
    path('api/', include('analysis.urls')),
]
