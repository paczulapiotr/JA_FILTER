
.data 
BYTE_IN_PIXEL dword 4
SUM REAL8 0.046322
.code
;int index, 		RCX 
;int arrayWidth,	RDX => EBX
;byte* original,	R8
;byte* filtered,	R9
;double* mask,		STACK [rbp+48] 
;int maskSize		STACK [rbp+56] 
;free registers: rax, rdx
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
;Set R,G,B sum registers
xorpd XMM0, XMM0
xorpd XMM1, XMM1
xorpd XMM2, XMM2
xorpd XMM3, XMM3
;set mask counter
xor r15d, r15d 
;set positionDiff to R10D
mov R10D, dword ptr[rbp+56]
sub R10D, 1
shr R10D, 1
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
jz ENDLOOP ;End loop if zero
inc r11d
XLOOP:
;X loop code start:
;calculate Index into eax
xor rax, rax
mov eax, r11d
imul ebx
add eax, r12d
imul BYTE_IN_PIXEL
add eax, ecx
;add to R 
	;zero temp register 
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
;add to B
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

ENDLOOP:

;Allocate byte in pixel
	cvtsd2si r10d, XMM0
	cvtsd2si r11d, XMM1
	cvtsd2si r12d, XMM2
	cvtsd2si r13d, XMM3
	mov byte ptr[R9+RCX], r10b
	mov byte ptr[R9+RCX+1], r11b
	mov byte ptr[R9+RCX+2], r12b
	mov byte ptr[R9+RCX+3], r13b

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

end 