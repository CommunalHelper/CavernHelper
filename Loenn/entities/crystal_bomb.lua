local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local crystalBomb = {}

crystalBomb.name = "cavern/crystalbomb"
crystalBomb.depth = -9999
crystalBomb.texture = "objects/cavern/crystalBomb/idle00"
crystalBomb.justification = {0.5, 0.5}

crystalBomb.placements = {
    name = "bomb",
    data = {
        respawnTime = 2.0,
        explodeTime = 1.0,
        explodeOnSpawn = false,
        respawnOnExplode = true,
        breakDashBlocks = false,
        legacyMode = false
    }
}

function crystalBomb.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 8, y - 3, 17, 13)
end

return crystalBomb