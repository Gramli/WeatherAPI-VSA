﻿namespace Weather.API.Domain.Database.Entities;

public sealed class FavoriteLocationEntity
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
