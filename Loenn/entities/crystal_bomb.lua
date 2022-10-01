local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local crystalBomb = {}

crystalBomb.name = "cavern/crystalbomb"
crystalBomb.depth = -9999
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

crystalBomb.fieldInformation = {
    respawnTime = {
        minimumValue = 0.0
    },
    explodeTime = {
        minimumValue = 0.0
    }
}

local offsetY = -10
local texture = "objects/cavern/crystalBomb/idle00"

function crystalBomb.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

function crystalBomb.rectangle(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 8, y - 13, 17, 13)
end

return crystalBomb