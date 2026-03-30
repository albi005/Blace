using Blace.Server.Data.Entities;
using Blace.Shared.Models;

namespace Blace.Server.Data.Mapping;

public static class EntityMapper
{
    // Place mappings
    public static Place ToModel(this PlaceEntity entity)
    {
        return new Place(
            entity.Id,
            entity.Title,
            entity.CreatedTimeUtc,
            entity.LastChangeTimeUtc,
            entity.Canvas,
            entity.Height,
            entity.Width);
    }

    public static PlaceEntity ToEntity(this Place model)
    {
        return new PlaceEntity
        {
            Id = model.Id,
            Title = model.Title,
            CreatedTimeUtc = model.CreatedTimeUtc,
            LastChangeTimeUtc = model.LastChangeTimeUtc,
            Canvas = model.Canvas,
            Height = model.Height,
            Width = model.Width
        };
    }

    public static TileEntity ToEntity(this Tile model)
    {
        return new TileEntity
        {
            Id = model.Id,
            CreatedTimeUtc = model.CreatedTimeUtc,
            PlaceId = model.PlaceId,
            UserId = model.UserId,
            X = model.X,
            Y = model.Y,
            Color = model.Color,
            PreviousColor = model.PreviousColor,
            DeleteId = model.DeleteId
        };
    }

    public static Delete ToModel(this DeleteEntity entity)
    {
        return new Delete(
            entity.Id,
            entity.DateTimeUtc,
            entity.UserId);
    }

    public static DeleteEntity ToEntity(this Delete model)
    {
        return new DeleteEntity
        {
            Id = model.Id,
            DateTimeUtc = model.DateTimeUtc,
            UserId = model.UserId
        };
    }
}
