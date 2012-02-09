vs_3_sw
    def c3056, 1, 2, 3, 4

dcl_position v0
dcl_normal v1
dcl_normal1 v3

dcl_color4     o3.x  
dcl_texcoord3  o3.yz 
dcl_fog        o3.w 
dcl_tangent    o4.xyz
dcl_position   o7.xyzw
dcl_psize      o6

#define oPos o7
#define oD0 o3

def c76, -10.0e5, 2.0e4, 3.3e2, 4.4
def c15, 1, 2, 3, 4
defi i1, 1, 2, 3,0
defb b11, true
defb b12, false

if_le v0.x, v1.y
nop
else
nop
endif

mova a0.yw, v0.argb
loop aL, i1
nop
break_le v1.x, r0.y
breakp !p0.y
nop
endloop

; Decompress position
mov r0.x, v0.x
mov r0.y, c4.w       ; 1
mov r0.z, v0.y
mov r0.w, c4.w       ; 1

setp_ge p0.yz, v1, v1

callnz l1, b11
callnz l1, !p0.w


if !p0.z
m3x2   r0.xy, r1, c0   ;which will be expanded to:
else
nop
endif

call l1

; Debug code [start]
ret
label l1
m3x2   r0.xy, r1, c0   ;which will be expanded to:
mov r0, r0.xz     
; Debug code [end]

; Compute theta from distance and time
mov r4.xz, r0        ; xz
mov r4.y, c4.y       ; y = 0
dp3 r4.x, r4, r4     ; d2
rsq r4.x, r4.x
rcp r4.x, r4.x       ; d
mul r4.xyz, r4, c4.x     ; scale by time

; Clamp theta to -pi..pi
add r4.x, r4.x, c7.x
mul r4.x, r4.x, c7.y
frc r4.xy, r4.x
mul r4.x, r4.x, c7.z
add r4.x, r4.x,-c7.x

; Compute first 4 values in sin and cos series
mov r5.x, c4.w       ; d^0
mov r4.x, r4.x       ; d^1
mul r5.y, r4.x, r4.x ; d^2
mul r4.y, r4.x, r5.y ; d^3
mul r5.z, r5.y, r5.y ; d^4
mul r4.z, r4.x, r5.z ; d^5
mul r5.w, r5.y, r5.z ; d^6
mul r4.w, r4.x, r5.w ; d^7

mul r4, r4, c10      ; sin
dp4 r4.x, r4, c4.w

mul r5, r5, c11      ; cos
dp4 r5.x, r5, c4.w

; Set color
add r5.x, -r5.x, c4.w ; + 1.0
mul oD0, r5.x, c4.z   ; * 0.5

; Scale height
mul r0.y, r4.x, c7.w

; Transform position
dp4 oPos.x, r0, c0
dp4 oPos.y, r0, c1
dp4 oPos.z, r0, c2
dp4 oPos.w, r0, c3

ret

