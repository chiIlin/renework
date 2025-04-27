from django.urls import path
from .views import total_losses, regions_losses, property_recommendations, google_search_test

urlpatterns = [
    # GET /api/losses/total/
    path('api/losses/total/', total_losses, name='total-losses'),
    path('api/losses/regions/', regions_losses, name='regions-losses'),
    path('api/business/<str:biz_id>/properties/', property_recommendations, name='biz-properties'),
    path('api/business/<str:biz_id>/google-search/', google_search_test, name='biz-google-search'),
]
