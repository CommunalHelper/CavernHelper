local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local crystalbomb = {}

crystalbomb.name = "cavern/crystalbomb"
crystalbomb.depth = -9999
crystalbomb.texture = "objects/cavern/crystalbomb/idle00"
crystalbomb.justification = {0.5, 0.5}
crystalbomb.placements = 
{
    name = "CrystalBomb",
    data = 
    {
        ["respawnTime"] = 2.0,
        ["explodeTime"] = 1.0,
        ["explodeOnSpawn"] = false,
        ["respawnOnExplode"] = true,
        ["breakDashBlocks"] = false
    }
}

function crystalbomb.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 8, y - 3, 17, 13)
end

return crystalbomb