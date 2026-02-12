using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace HouseNet9.Helpers
{
    public static class ReservationHelper
    {
        public static (DateTime from, DateTime to, int houseId) GetReservationFromTempData(ITempDataDictionary tempData)
        {
            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MinValue;
            int houseId = 0;

            // Obsługa From
            if (tempData.TryGetValue("From", out var fromObj))
            {
                if (fromObj is DateTime dt) from = dt;
                else if (fromObj is string s && DateTime.TryParse(s, out var parsed)) from = parsed;
            }

            // Obsługa To
            if (tempData.TryGetValue("To", out var toObj))
            {
                if (toObj is DateTime dt) to = dt;
                else if (toObj is string s && DateTime.TryParse(s, out var parsed)) to = parsed;
            }

            // Obsługa HouseId
            if (tempData.TryGetValue("HouseId", out var houseObj))
            {
                if (houseObj is int h) houseId = h;
                else if (houseObj is string s && int.TryParse(s, out var parsed)) houseId = parsed;
            }

            return (from, to, houseId);
        }
    }

}
