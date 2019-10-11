
EXTERN OnEnter:PROC
EXTERN OnLeave:PROC
EXTERN OnTailCall:PROC

_text SEGMENT PARA 'CODE'

ALIGN 16
PUBLIC EnterNakedFunc

EnterNakedFunc PROC FRAME

    PUSH RAX
    .PUSHREG RAX
    PUSH RCX
    .PUSHREG RCX
    PUSH RDX
    .PUSHREG RDX
    PUSH R8
    .PUSHREG R8
    PUSH R9
    .PUSHREG R9
    PUSH R10
    .PUSHREG R10
    PUSH R11
    .PUSHREG R11

    SUB RSP, 20H
    .ALLOCSTACK 20H

    .ENDPROLOG

    CALL OnEnter

    ADD RSP, 20H

    POP R11
    POP R10
    POP R9
    POP R8
    POP RDX
    POP RCX
    POP RAX

    RET

EnterNakedFunc ENDP

ALIGN 16
PUBLIC LeaveNakedFunc

LeaveNakedFunc PROC FRAME

    PUSH RAX
    .PUSHREG RAX
    PUSH RCX
    .PUSHREG RCX
    PUSH RDX
    .PUSHREG RDX
    PUSH R8
    .PUSHREG R8
    PUSH R9
    .PUSHREG R9
    PUSH R10
    .PUSHREG R10
    PUSH R11
    .PUSHREG R11

    SUB RSP, 20H
    .ALLOCSTACK 20H

    .ENDPROLOG

    CALL OnLeave

    ADD RSP, 20H

    POP R11
    POP R10
    POP R9
    POP R8
    POP RDX
    POP RCX
    POP RAX

    RET

LeaveNakedFunc ENDP

ALIGN 16
PUBLIC TailCallNakedFunc

TailCallNakedFunc PROC FRAME

    PUSH RAX
    .PUSHREG RAX
    PUSH RCX
    .PUSHREG RCX
    PUSH RDX
    .PUSHREG RDX
    PUSH R8
    .PUSHREG R8
    PUSH R9
    .PUSHREG R9
    PUSH R10
    .PUSHREG R10
    PUSH R11
    .PUSHREG R11

    SUB RSP, 20H
    .ALLOCSTACK 20H

    .ENDPROLOG

    CALL OnTailCall

    ADD RSP, 20H

    POP R11
    POP R10
    POP R9
    POP R8
    POP RDX
    POP RCX
    POP RAX

    RET

TailCallNakedFunc ENDP

_text ENDS

END
