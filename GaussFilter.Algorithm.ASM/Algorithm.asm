.data 
; amount of bytes in one pixel
BYTE_IN_PIXEL dword 4
BYTE_IN_PIXEL_Q qword 4
ZERO dword 0
.code
; ==============================================================
; Main procedure used for blurring one pixel image row, 
; right and left border is not included.
; ### PARAMETERS ###
;	int width,						RCX => [rbp+60]			[image pixel width]
;	int height,	    				RDX => [rbp+64]			[image pixel height]
; 	int endSubpixelIndex			[rbp+68]				[used for end condition for set_row_to_black proc and main loop]
;	byte* original,					R8						[byte array of original image]
;	byte* filtered,					R9						[byte array of filtered image]
;	int* mask,		    			STACK [rbp+48] 			[laplace mask table pointer]
;	int subPixelsCount,				STACK [rbp+56] 			[image pixels count]
; ### USED REGISTERS ###
; rax current subpixel index 
; r8 
; r9 

; ### FREE REGISTERS ###
; rbx,rcx,rdx,r10,r11,r12,r13,r14,r15
; ==============================================================

laplace proc
; prepare for stack deallocation
push rbp
mov rbp, rsp
push rax
push rbx
push rcx
push rdx
push r12
push r13
push r14
push r15
; main code
PREPAREVARIABLES:
mov [rbp+60], ecx
mov [rbp+64], edx

; set top row to black
mov edx, dword ptr[rbp+60]
imul edx, 4
mov [rbp+68], edx

xor rax, rax
call set_row_to_black

; set bottom row to black
mov r15d, dword ptr[rbp+56]
sub r15d, dword ptr[rbp+60]
sub r15d, dword ptr[rbp+60]
sub r15d, dword ptr[rbp+60]
sub r15d, dword ptr[rbp+60]
; set start index of row to black out
mov eax, r15d
mov r14d, dword ptr[rbp+56]
mov [rbp+68], r14d
call set_row_to_black

; set end index for main loop
mov [rbp+68], r15d

; set index to second row
mov eax, dword ptr[rbp+60]
shl eax, 2

; run main loop
MAINLOOP:
; set first pixel from row to black
call set_pixel_to_black

mov r15d, dword ptr[rbp+60]
mov r14d, 2

mov rbx, rax
add rbx, r8
xor rcx, rcx
xor rdx, rdx
INSIDEROW:
	
xor r10, r10
; R
	; add 1st subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx]
		add r10d, ecx
	; add 2nd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+4]
		add r10d, ecx
	; add 3rd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+8]
		add r10d, ecx
	; add 4th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+12]
		add r10d, ecx
	; add 5th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+16]
		add r10d, ecx
	; add 6th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+20]
		add r10d, ecx
	; add 7th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+24]
		add r10d, ecx
	; add 8th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+28]
		add r10d, ecx
	; add 9th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor ecx, ecx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+32]
		add r10d, ecx


	; check max/min for subpixels

	cmp r10d, 255
	jg SET_MAX_R
	cmp r10d, 0
	jl SET_MIN_R
	jmp SET_R

	SET_MAX_R:
	mov r10, 255
	jmp SET_R

	SET_MIN_R:
	mov r10, 0
	jmp SET_R

	SET_R:
	mov rcx, r10
	mov byte ptr[r9+rax], cl
	; set subpixels

xor r10, r10
inc rbx
inc rax
; G
	; add 1st subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx]
		add r10d, ecx
	; add 2nd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+4]
		add r10d, ecx
	; add 3rd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+8]
		add r10d, ecx
	; add 4th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+12]
		add r10d, ecx
	; add 5th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+16]
		add r10d, ecx
	; add 6th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+20]
		add r10d, ecx
	; add 7th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+24]
		add r10d, ecx
	; add 8th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+28]
		add r10d, ecx
	; add 9th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+32]
		add r10d, ecx


	; check max/min for subpixels

	cmp r10d, 255
	jg SET_MAX_G
	cmp r10d, 0
	jl SET_MIN_G
	jmp SET_G

	SET_MAX_G:
	mov r10, 255
	jmp SET_G

	SET_MIN_G:
	mov r10, 0
	jmp SET_G

	SET_G:
	mov rcx, r10
	mov byte ptr[r9+rax], cl
	; set subpixels

xor r10, r10
inc rbx
inc rax
; B
	; add 1st subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx]
		add r10d, ecx
	; add 2nd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+4]
		add r10d, ecx
	; add 3rd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+8]
		add r10d, ecx
	; add 4th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+12]
		add r10d, ecx
	; add 5th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+16]
		add r10d, ecx
	; add 6th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+20]
		add r10d, ecx
	; add 7th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+24]
		add r10d, ecx
	; add 8th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+28]
		add r10d, ecx
	; add 9th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+32]
		add r10d, ecx


	; check max/min for subpixels

	cmp r10d, 255
	jg SET_MAX_B
	cmp r10d, 0
	jl SET_MIN_B
	jmp SET_B

	SET_MAX_B:
	mov r10, 255
	jmp SET_B

	SET_MIN_B:
	mov r10, 0
	jmp SET_B

	SET_B:
	mov rcx, r10
	mov byte ptr[r9+rax], cl
	; set subpixels

xor r10, r10
inc rbx
inc rax
; A
	; add 1st subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx]
		add r10d, ecx
	; add 2nd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+4]
		add r10d, ecx
	; add 3rd subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		sub rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+8]
		add r10d, ecx
	; add 4th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+12]
		add r10d, ecx
	; add 5th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+16]
		add r10d, ecx
	; add 6th subpixels from mask
		mov rdx, rbx
		xor rcx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+20]
		add r10d, ecx
	; add 7th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		sub rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+24]
		add r10d, ecx
	; add 8th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+28]
		add r10d, ecx
	; add 9th subpixels from mask
		mov rdx, rbx
		mov ecx, dword ptr[rbp+60]
		imul rcx, BYTE_IN_PIXEL_Q
		add rdx, rcx
		add rdx, BYTE_IN_PIXEL_Q
		; get corresponding image subpixel
		xor rcx, rcx
		mov cl, byte ptr[rdx]
		; get mask pointer to edx
		mov rdx, qword ptr[rbp+48]
		; multiply mask value with 
		imul ecx, dword ptr[rdx+32]
		add r10d, ecx


	; check max/min for subpixels

	cmp r10d, 255
	jg SET_MAX_A
	cmp r10d, 0
	jl SET_MIN_A
	jmp SET_A

	SET_MAX_A:
	mov r10, 255
	jmp SET_A

	SET_MIN_A:
	mov r10, 0
	jmp SET_A

	SET_A:
	mov rcx, r10
	mov byte ptr[r9+rax], cl
	; set subpixels


	; loop conditions
	inc r14d
	inc rax
	cmp r14d, r15d
	jz ENDINSIDEROW
	jmp INSIDEROW
ENDINSIDEROW:

call set_pixel_to_black

; set last pixel from row to black

	cmp eax, dword ptr[rbp+56]
	jz ENDMAINLOOP
	jmp MAINLOOP
ENDMAINLOOP:

CLEARSTACK:
; clear stack
	pop r15
	pop r14
	pop r13
	pop r12
	pop rdx
	pop rcx
	pop rbx
	pop rax
	pop rbp
	ret
laplace endp 


set_pixel_to_black proc
	mov edx, 0
	mov rbx, rax
	add rbx, r9
	;set R
	mov dword ptr[rbx], edx
	inc rbx
	;set G
	mov dword ptr[rbx], edx
	inc rbx
	;set B
	mov dword ptr[rbx], edx
	inc rbx
	;set A
	mov dword ptr[rbx], edx
	add eax, BYTE_IN_PIXEL
	ret
set_pixel_to_black endp

set_row_to_black proc

;set first row to black
mov edx, 0
FIRSTROWLOOP:
	mov rbx, rax
	add rbx, r9
	;set R
	mov dword ptr[rbx], edx
	inc rbx
	;set G
	mov dword ptr[rbx], edx
	inc rbx
	;set B
	mov dword ptr[rbx], edx
	inc rbx
	;set A
	mov dword ptr[rbx], edx
	add eax, BYTE_IN_PIXEL
	cmp eax, dword ptr[rbp+68]
	jz ENDFIRSTROWLOOP
	jmp FIRSTROWLOOP
ENDFIRSTROWLOOP:
	ret

set_row_to_black endp




border proc
; preconditions
	xor rbx, rbx
XLOOP:
; set top border
	mov r11d, dword ptr [rcx+rbx]
	mov dword ptr [rdx+rbx], r11d
; set bottom boder
	mov r10, rcx
	add r10, r9
	mov r11d, dword ptr[r10 + rbx]
	mov r10, rdx
	add r10, r9
	mov dword ptr [r10+rbx], r11d
; increment loop
add ebx, BYTE_IN_PIXEL
cmp r8d, ebx
jz FINISHED
jmp XLOOP

FINISHED:
ret
border endp
end 