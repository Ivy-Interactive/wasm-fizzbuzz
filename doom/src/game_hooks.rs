use std::os::raw::c_int;

use crate::js_imports;

extern "C" {
    // Defined in i_hooks.c — fills doom_state_buffer with current player state
    fn doom_fill_state_buffer();
    static doom_state_buffer: [c_int; 8];
}

/// Called from DOOM C code when the player fires a weapon.
#[no_mangle]
pub extern "C" fn doom_hook_weapon_fired(weapon: c_int) {
    unsafe { js_imports::js_on_weapon_fired(weapon) };
}

/// Called from DOOM C code when the player kills an enemy.
#[no_mangle]
pub extern "C" fn doom_hook_enemy_killed(enemy_type: c_int, killer_weapon: c_int) {
    unsafe { js_imports::js_on_enemy_killed(enemy_type, killer_weapon) };
}

/// Exported for JS to poll player state each frame.
/// Calls into C to fill the buffer, then returns a pointer to it.
/// Layout: [health, armorpoints, armortype, readyweapon, ammo_clip, ammo_shell, ammo_cell, ammo_misl]
#[no_mangle]
pub extern "C" fn get_player_state() -> *const c_int {
    unsafe {
        doom_fill_state_buffer();
        doom_state_buffer.as_ptr()
    }
}
