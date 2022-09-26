module CavernFakeCavernHeart

using ..Ahorn, Maple

@mapdef Entity "cavern/fakecavernheart" FakeCavernHeart(x::Integer, y::Integer)

function Ahorn.selection(entity::FakeCavernHeart)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 8, y - 8, 16, 16)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FakeCavernHeart) = Ahorn.drawSprite(ctx, "collectables/heartGem/0/00.png", 0, 0, jx=0.5, jy=0.5)

end