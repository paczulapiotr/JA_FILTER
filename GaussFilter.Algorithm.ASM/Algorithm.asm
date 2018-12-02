
.data 

.code

;byte* original	RCX 
;byte* filtered RDX
;int maskSize	R8D
;double* mask	R9
;int index		STACK
gauss proc
;Prepare for stack deallocation
push rbp
mov rbp, rsp
;Get index to eax register
mov eax, dword ptr[rsp+40]
;Set R,G,B sum registers
xorpd XMM0, XMM0
xorpd XMM1, XMM1
xorpd XMM2, XMM2
;set mask counter
xor ebx, ebx 
; ;set positionDiff to R10D
mov R10D, r8d
sub R10D, 1
shr R10D, 1
; ;y param for loop
mov r11d, r10d
neg r11d
;x param for loop
RESETX:
mov r12d, R10D
neg r12d

YLOOP:
cmp r11d, r9d
jz ENDLOOP ;End loop if zero
inc r11d
XLOOP:
; ;X loop code start:




; ;X loop code end
cmp r12d, R10D
jz RESETX ;
inc r12d
jmp XLOOP

ENDLOOP:
; ;Clear stack
mov rsp, rbp
pop rbp
ret
gauss endp 



end 