module Cavern

using ..Ahorn, Maple

@mapdef Entity "cavern/crystalbomb" CrystalBomb(x::Integer, y::Integer)
@mapdef Entity "cavern/icyfloor" IcyFloor(x::Integer, y::Integer, width::Integer=8)
@mapdef Entity "cavern/fakecavernheart" FakeCavernHeart(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Crystal Bomb (Cavern)" => Ahorn.EntityPlacement(
        CrystalBomb,
        "point",
        Dict{String, Any}(
            "respawnTime" => 2.0,
            "explodeTime" => 1.0,
            "explodeOnSpawn" => false,
            "respawnOnExplode" => true,
            "breakDashBlocks" => false
        )
    ),
    "Icy Floor (Cavern)" => Ahorn.EntityPlacement(
        IcyFloor,
        "rectangle",
        Dict{String, Any}()
    ),
    "Fake Cavern Heart (Cavern)" => Ahorn.EntityPlacement(
        FakeCavernHeart,
        "point"
    )
)

Ahorn.minimumSize(entity::IcyFloor) = 8, 0
Ahorn.resizable(entity::IcyFloor) = true, false

function Ahorn.selection(entity::CrystalBomb)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("objects/cavern/crystalbomb/idle00.png", x, y, jx=0.5, jy=0.5)
end

function Ahorn.selection(entity::IcyFloor)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.selection(entity::FakeCavernHeart)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 8, y - 8, 16, 16)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FakeCavernHeart) = Ahorn.drawSprite(ctx, "collectables/heartGem/0/00.png", 0, 0, jx=0.5, jy=0.5)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalBomb) = Ahorn.drawSprite(ctx, "objects/cavern/crystalbomb/idle00.png", 0, -3, jx=0.5, jy=0.5)

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