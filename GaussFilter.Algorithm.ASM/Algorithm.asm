.data 
BYTE_IN_PIXEL dword 4
.code
;int index, 		RCX 
;int arrayWidth,	RDX => EBX
;byte* original,	R8
;byte* filtered,	R9
;double* mask,		STACK [rbp+48] 
;int maskSize		STACK [rbp+56] 
;free registers: rax, rdx
;positionDiff		R10D
;Y loop param		R11D
;X loop param		R12D
;Row loop param		R13D
;realWidth			R14D
;maskCounter 		R15D
gauss proc
;Prepare for stack deallocation
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
;arrayWidth dword ptr[rbp+40]
;index dword ptr[rbp+48]
;mov arrayWidth
mov ebx, edx
;set positionDiff to R10D
mov R10D, dword ptr[rbp+56]
sub R10D, 1
shr R10D, 1
;set realWidth to R14D
mov eax, R10D
shl eax, 1
sub edx, eax
mov eax, edx
mul BYTE_IN_PIXEL
mov R14D,eax

;row param for loop
xor r13d, r13d
ROWLOOP:
;set mask counter
	xor r15d, r15d 
;Set R,G,B sum registers
	xorpd XMM0, XMM0
	xorpd XMM1, XMM1
	xorpd XMM2, XMM2
	xorpd XMM3, XMM3

;y param for loop
	mov r11d, r10d
	neg r11d
	dec r11d

;x param for loop
RESETX:
	mov r12d, R10D
	neg r12d

YLOOP:
	cmp r11d, R10D
	jz SETPIXEL ;End loop if zero
	inc r11d
XLOOP:
;X loop code start:
;calculate Index into eax
	mov eax, r11d
	imul ebx
	add eax, r12d
	imul BYTE_IN_PIXEL
	add eax, ecx
;add to R 
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM0, XMM4
;increment index
	inc eax
;add to G
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM1, XMM4
;increment index
	inc eax
;add to B
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM2, XMM4
;increment index
	inc eax
;add to A
	xorpd XMM4, XMM4
	xor edx, edx
	mov dl, byte ptr[r8 + rax]
	CVTSI2SD XMM4, edx
	xorpd XMM5, XMM5
	mov rdx, qword ptr[rbp+48]
	movsd XMM5, REAL8 ptr[rdx + r15]
	mulsd XMM4, XMM5
	addsd XMM3, XMM4
;increment maskCounter
	add r15d, 8

;X loop code end
	cmp r12d, R10D
	jz RESETX ;
	inc r12d
	jmp XLOOP

SETPIXEL:

;Allocate byte in pixel
;set R
mov rdx, r9
add rdx, rcx
;TODO:incremet rcx boy
cvtsd2si eax, XMM0
mov byte ptr[rdx], al
;set G
inc rdx
cvtsd2si eax, XMM1
mov byte ptr[rdx], al
;set B
inc rdx
cvtsd2si eax, XMM2
mov byte ptr[rdx], al
;set A
inc rdx
cvtsd2si eax, XMM3
mov byte ptr[rdx], al
;increment row loop param
add R13D, BYTE_IN_PIXEL
;increment current index in bitmap
add ECX, BYTE_IN_PIXEL
cmp R13D,R14D
jz CLEARSTACK
jmp ROWLOOP

CLEARSTACK:
;Clear stack
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
gauss endp 

;ecx => r10d original
;edx => r11d filtered
;r8d topBottomBorderSize
;r9d bottomBoundStartIndex
;ebx loop counter

border proc
;Preconditions
	xor rbx, rbx
XLOOP:
;set top border
	mov r11d, dword ptr [rcx+rbx]
	mov dword ptr [rdx+rbx], r11d
;set bottom boder
	mov r10, rcx
	add r10, r9
	mov r11d, dword ptr[r10 + rbx]
	mov r10, rdx
	add r10, r9
	mov dword ptr [r10+rbx], r11d
;increment loop
add ebx, BYTE_IN_PIXEL
cmp r8d, ebx
jz FINISHED
jmp XLOOP

FINISHED:
ret
border endp
end 