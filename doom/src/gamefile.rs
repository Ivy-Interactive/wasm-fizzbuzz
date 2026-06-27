use std::ffi::CStr;
use std::os::raw::{c_char, c_int};

const WAD_FD: c_int = 42; // file descriptor for the loaded WAD

static mut WAD_DATA: *const u8 = std::ptr::null();
static mut WAD_SIZE: usize = 0;
static mut WAD_SEEKER: usize = 0;

// Which WAD type to emulate (set by JS before calling main).
// 0 = doom1 (shareware), 1 = doom (registered), 2 = doom2 (commercial)
static mut WAD_TYPE: u8 = 0;

/// Called from JS to allocate a buffer for the WAD file.
/// Returns a pointer where JS should write the WAD data into wasm memory.
#[no_mangle]
pub extern "C" fn alloc_wad(size: usize) -> *mut u8 {
    unsafe {
        let layout = std::alloc::Layout::from_size_align(size, 1).unwrap();
        let ptr = std::alloc::alloc(layout);
        WAD_DATA = ptr;
        WAD_SIZE = size;
        WAD_SEEKER = 0;
        ptr
    }
}

/// Called from JS to set which game type the WAD is.
/// 0 = doom1 (shareware), 1 = doom (registered/ultimate), 2 = doom2 (commercial)
#[no_mangle]
pub extern "C" fn set_wad_type(wad_type: u8) {
    unsafe { WAD_TYPE = wad_type; }
}

fn wad_slice() -> &'static [u8] {
    unsafe {
        assert!(!WAD_DATA.is_null(), "WAD not loaded! Call alloc_wad and write WAD data before main().");
        std::slice::from_raw_parts(WAD_DATA, WAD_SIZE)
    }
}

static HOME_ENV: &'static [u8; 11] = b"/home/doom\0"; // C string, terminate with \0!

#[no_mangle]
extern "C" fn getenv(name: *const c_char) -> *const c_char {
    let name = unsafe { CStr::from_ptr(name) };
    let name = name.to_str().expect("invalid UTF8 getenv call");
    let result = match name {
        "DOOMWADDIR" => std::ptr::null(),
        "HOME" => HOME_ENV.as_ptr() as *const c_char,
        _ => {
            crate::log!("unexepcted getenv({:?}) call", name);
            std::ptr::null()
        }
    };
    result
}

#[no_mangle]
extern "C" fn access(pathname: *const c_char, _mode: c_int) -> c_int {
    const ENOENT: c_int = 2;

    let pathname = unsafe { CStr::from_ptr(pathname).to_str().expect("invalid UTF8") };
    let wad_type = unsafe { WAD_TYPE };

    // The engine probes these filenames in order to determine game mode.
    // Return success for the filename matching our WAD type.
    match pathname {
        "./doom2f.wad" | "./doom2.wad" | "./plutonia.wad" | "./tnt.wad" if wad_type == 2 => 0,
        "./doom.wad" | "./doomu.wad" if wad_type == 1 => 0,
        "./doom1.wad" if wad_type == 0 => 0,
        // Return not-found for everything else
        "./doom2f.wad" | "./doom2.wad" | "./plutonia.wad" | "./tnt.wad"
        | "./doom.wad" | "./doomu.wad" | "./doom1.wad" => ENOENT,
        _ => panic!("access({}, {}) unimplemented", pathname, _mode),
    }
}

#[no_mangle]
extern "C" fn fopen(pathname: *const c_char, mode: c_int) -> i32 /* FILE* */ {
    let pathname = unsafe { CStr::from_ptr(pathname).to_str().expect("invalid UTF8") };

    if pathname == "/home/doom/.doomrc" {
        return 0; // NULL for error
    }

    panic!("fopen({}, {}) unimplemented", pathname, mode);
}

#[no_mangle]
extern "C" fn open(pathname: *const c_char, flags: c_int, mode: i32) -> i32 {
    let pathname = unsafe { CStr::from_ptr(pathname).to_str().expect("invalid UTF8") };

    match pathname {
        "./doom1.wad" | "./doom.wad" | "./doomu.wad"
        | "./doom2.wad" | "./doom2f.wad" | "./plutonia.wad" | "./tnt.wad" => WAD_FD,
        _ => panic!("open({}, {}, {}) unimplemented", pathname, flags, mode),
    }
}

#[no_mangle]
extern "C" fn read(fd: c_int, buf: *mut u8, count: usize) -> isize {
    if fd == WAD_FD {
        let wad = wad_slice();
        let buf = unsafe { std::slice::from_raw_parts_mut(buf, count) };
        let s = unsafe { WAD_SEEKER };
        buf[..count].copy_from_slice(&wad[s..s + count]);
        unsafe {
            WAD_SEEKER += count;
        }
        return count as isize;
    }
    panic!("read({}, buf, {}) unimplemented", fd, count);
}

#[no_mangle]
extern "C" fn lseek(fd: i32, offset: i64, whence: c_int) -> i64 {
    const SEEK_SET: c_int = 0;
    const _SEEK_CUR: c_int = 1;
    const _SEEK_END: c_int = 2;
    if fd == WAD_FD {
        match whence {
            SEEK_SET => {
                unsafe { WAD_SEEKER = offset as usize };
                return unsafe { WAD_SEEKER } as i64;
            }
            _ => {
                crate::log!("TODO lseek");
            }
        }
    }
    panic!("lseek({}, {}, {}) unimplemented", fd, offset, whence);
}
