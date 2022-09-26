module CavernIcyFloor

using ..Ahorn, Maple

@mapdef Entity "cavern/icyfloor" IcyFloor(x::Integer, y::Integer, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "Icy Floor (Cavern)" => Ahorn.EntityPlacement(
        IcyFloor,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::IcyFloor) = 8, 0
Ahorn.resizable(entity::IcyFloor) = true, false

function Ahorn.selection(entity::IcyFloor)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))
    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IcyFloor, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))
    tileWidth = div(width, 8)

    Ahorn.Cairo.save(ctx)
    Ahorn.scale(ctx, -1, 1)
    Ahorn.rotate(ctx, -pi/2)

    for i in 2:tileWidth - 1
        Ahorn.drawImage(ctx, "objects/wallBooster/iceMid00", -8, (i - 1) * 8 - width)
    end

    Ahorn.drawImage(ctx, "objects/wallBooster/iceTop00", -8, -width)
    Ahorn.drawImage(ctx, "objects/wallBooster/iceBottom00", -8, (tileWidth - 1) * 8 - width)

    Ahorn.restore(ctx)
end

end