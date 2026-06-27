#include "doomdef.h"
#include "doomstat.h"
#include "d_player.h"

// Buffer that Rust/JS reads for player state.
// Layout: [health, armorpoints, armortype, readyweapon, ammo_clip, ammo_shell, ammo_cell, ammo_misl]
int doom_state_buffer[8];

void doom_fill_state_buffer(void)
{
    player_t *p = &players[0];
    doom_state_buffer[0] = p->health;
    doom_state_buffer[1] = p->armorpoints;
    doom_state_buffer[2] = p->armortype;
    doom_state_buffer[3] = p->readyweapon;
    doom_state_buffer[4] = p->ammo[am_clip];
    doom_state_buffer[5] = p->ammo[am_shell];
    doom_state_buffer[6] = p->ammo[am_cell];
    doom_state_buffer[7] = p->ammo[am_misl];
}
