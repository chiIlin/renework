from django.urls import path, include

urlpatterns = [
    # … ваші інші маршрути …
    path('', include('analysis.urls')),  # <— тут мається відповідати шляху до вашого urls.py
]
