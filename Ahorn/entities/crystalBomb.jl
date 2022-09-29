module CavernCrystalBomb

using ..Ahorn, Maple

@mapdef Entity "cavern/crystalbomb" CrystalBomb(x::Integer, y::Integer, respawnTime::Number=2.0, explodeTime::Number=1.0, explodeOnSpawn::Bool=false, respawnOnExplode::Bool=true, breakDashBlocks::Bool=false, legacyMode::Bool=false)

const placements = Ahorn.PlacementDict(
    "Crystal Bomb (Cavern)" => Ahorn.EntityPlacement(
        CrystalBomb,
        "point"
    )
)

sprite = "objects/cavern/crystalbomb/idle00.png"

function Ahorn.selection(entity::CrystalBomb)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 10, y - 14, 19, 15)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalBomb) = Ahorn.drawSprite(ctx, sprite, 0, -10)

end