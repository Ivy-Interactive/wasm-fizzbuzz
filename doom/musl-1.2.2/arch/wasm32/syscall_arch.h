#define __SYSCALL_LL_E(x) (x)
#define __SYSCALL_LL_O(x) (x)

/* wasm32 has no syscalls — all I/O is handled by Rust/JS glue.
   Return -ENOSYS (38) for any syscall that gets called. */
static inline long __syscall0(long n) { return -38; }
static inline long __syscall1(long n, long a) { return -38; }
static inline long __syscall2(long n, long a, long b) { return -38; }
static inline long __syscall3(long n, long a, long b, long c) { return -38; }
static inline long __syscall4(long n, long a, long b, long c, long d) { return -38; }
static inline long __syscall5(long n, long a, long b, long c, long d, long e) { return -38; }
static inline long __syscall6(long n, long a, long b, long c, long d, long e, long f) { return -38; }
