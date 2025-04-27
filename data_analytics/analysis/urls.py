from django.urls import path
from .views import total_losses, regions_losses, property_recommendations

urlpatterns = [
    # GET /api/losses/total/
    path('api/losses/total/', total_losses, name='total-losses'),
    path('api/losses/regions/', regions_losses, name='regions-losses'),
    path('api/business/<str:biz_id>/properties/', property_recommendations, name='biz-properties'),
]
