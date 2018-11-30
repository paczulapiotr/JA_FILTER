
.data 
value db 20
max dw 255
.code

gauss proc
;RCX - data array
;RDX - array orignal
;R8D - size
;R9D
mov rbx, 0
MainLoop:
dec R8D
jz LoopEnd
mov al, byte ptr[rcx+rbx]
add al, value
cmovc ax, max
mov byte ptr[rdx+rbx], al
inc rbx
jmp MainLoop

LoopEnd:
ret




gauss endp 



end 