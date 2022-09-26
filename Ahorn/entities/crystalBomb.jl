module CavernCrystalBomb

using ..Ahorn, Maple

@mapdef Entity "cavern/crystalbomb" CrystalBomb(x::Integer, y::Integer, respawnTime::Number=2.0, explodeTime::Number=1.0, explodeOnSpawn::Bool=false, respawnOnExplode::Bool=true, breakDashBlocks::Bool=false, legacyMode::Bool=false)

const placements = Ahorn.PlacementDict(
    "Crystal Bomb (Cavern)" => Ahorn.EntityPlacement(
        CrystalBomb,
        "point"
    )
)

function Ahorn.selection(entity::CrystalBomb)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 10, y - 7, 19, 15)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalBomb) = Ahorn.drawSprite(ctx, "objects/cavern/crystalbomb/idle00.png", 0, -3, jx=0.5, jy=0.5)

end