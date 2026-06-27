// JavaScript imports.

#[link(wasm_import_module = "js")]
extern "C" {
    pub fn js_console_log(ptr: *const u8, len: usize);
    pub fn js_stdout(ptr: *const u8, len: usize);
    pub fn js_stderr(ptr: *const u8, len: usize);
    pub fn js_on_weapon_fired(weapon: i32);
    pub fn js_on_enemy_killed(enemy_type: i32, killer_weapon: i32);
}
