REBOL [
    title: {TAB text format parser, converts to set of nested blocks}
    issues: {
        need to get longer rhythm elements even when shorter one matches
            usually this just goes on its own line "e.g. "b" vs "bT" - DONE
            
        
        need a way to have bourdons as /a //a ///a 4 5 6 7
        (this occurrs in position 7 only) - DONE
        
        need to basic apply cell formatting hints in the output (e.g. cell
        borders to simulate staves and barlines
        
        need some way to indicate which lines werent parsed (for whatever reason)
        
        need to decide what should be emitted when lines dont parse
                
    }
    
    history: {
        v01d -  basically working
        v01e - refactored for modularisation
        
        Apr 2017
            Many improvements
            Handles Capriola ghost notes
            RH fingering now attached to correct position
            better parsing of headers (not limited to 7 items)
    }
    
    notes: {
    
    from Wayne to clarify optionality of pre/postfix
    
     (not speaking of flags) a note can have one "ornament" in
    front of it, and one "ornament" in back of it, and one fingering
    in front of it.  
    
    }
]


;===================================================
;
;    LuteScribe, a GUI tool to view and edit lute tabulature 
;    (and for related plucked instruments).
;
;    Copyright (C) 2018, Luke Emmet 
;
;    Email: luke [dot] emmet [at] orlando-lutes [dot] com
;
;    This program is free software: you can redistribute it and/or modify
;    it under the terms of the GNU General Public License as published by
;    the Free Software Foundation, either version 3 of the License, or
;    (at your option) any later version.
;
;    This program is distributed in the hope that it will be useful,
;    but WITHOUT ANY WARRANTY; without even the implied warranty of
;    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;    GNU General Public License for more details.
;
;    You should have received a copy of the GNU General Public License
;    along with this program.  If not, see <https://www.gnu.org/licenses/>.
;
;===================================================


bar-lines: [some ["." | "b" | "B"]]


parse-tab: func [input-string] [

    extract-mark-ending: does [append notes copy/part mark ((index? ending) - (index? mark))]
    
    notes: copy []
     ignored: off
    
    rhythm-element-basic: ["L" | "B" | "W" | "w" | "0" | "1" | "2" | "3" |  "4" | "5" ]
    rhythm-modifier: charset ".#!*"
    rhythm-element: [rhythm-element-basic opt rhythm-modifier]
        
    bar-lines: [some ["." | "b" | "B"]]
    
    other-rhythm-element: [
        ;---some more of these - just for testing
        "bT" | "b!" | "bv" | "bV" | "B!"  |
        
          ;first or second ending
          "A1" | "A2" |
          
        bar-lines
;        "B" | 
;        "b." | ".bb" | ".b.b" | "bb." | "b" | ".bb." |
        "C" | "c" |
        "d" | "D" |
        
        "e" | 
        ;---other ways of doing a grid
        "#1" | "#2" | "#3" | "#4" | "#5" |
        
          ;--Capirola "ghost" notes
           "O" |

        ;---invisible flags (probably better way to do this)
        "!L" | "!W" | "!w" | "!0" | "!1" | "!2" | "!3" | "!4" |

        "-1" | "-2" | "-3" | "-4" | "-5" |
        
        ;---more of these...
        "F 1" | "F 2" | 
        "f" | 
        
        "M" | "m" | 
        ;"." | 
        "x" |
        "y" | "Y" | "Yb" | "yb" | "YW" | "Yw" | "yW" | "yw" | "Y." |
        
        "R1" | "R2" | 
        "t1" | "t2" | 
        "S2" | "S3" | "S4" | "S5" | "S6" | "S8" |
        
        "Q" | "q" |
        
          ;--indent a bit
          "i" |
          
        "~"
        
        ;above-coment
        
        ;---etc
    ]
    
    ornaments: charset join ".:|-+*#x$<'`Q%?>@=_wuU(){}[]" {"}
    insert ornaments #"^^"    ;---trying to insert "^" into ornaments bitset...
    insert ornaments #"]"
    insert ornaments #"."
    
    fret: charset "abcdefghijklmnop1234567890"
    insert fret #" "

    bourdon-slash: #"/"
    
    left-digit: charset [#"1" - #"4"]
    number: charset [#"0" - #"9"]
    rh-fingering: charset ".:|;"        ;---should also permit "||" 
    
    fingering: ["\" left-digit]
    
    prefix: [opt {"} ornaments]
    
    prefixed-ornament: [opt {"} ornaments]
    postfixed-ornament: [opt "&" [ 0 2 ornaments]]
    
    postfix: ["&" [ornaments | fingering]]
    
    ;any bourdon-slash
    fret2: [
        [0 1 fingering]
        [0 1 prefixed-ornament]

        fret 
        [0 1 postfixed-ornament]
          [0 2 rh-fingering]            ;---2 to accomodate "||"
    ]
    
    
    bourdon-element: [
        [0 1 prefixed-ornament]
        any bourdon-slash
        [fret | "4" | "5" | "6" | "7"]
        [0 1 postfixed-ornament]
    ]
    
    mark-prev: 1
    
            
    parse-success: parse/all input-string [
            
        ;---collect rhythm-element
        mark: [rhythm-element  | other-rhythm-element] ending: (extract-mark-ending)
        
        ;---seperatory between rhythm element and fret content is optional
        [0 1 "-"]
        
        some [
            ;---previously [mark: fret2 ending: ] (
            [mark: fret2 ending:] (                
                ;---extract the fragment from the mark to the ending
                extract-mark-ending
                
                )   
        ] 
        
        ;---we might end in a bourdon element which is different to a normal fret.
        any [
            [mark: bourdon-element ending: (extract-mark-ending)]
        
        ]

        end        
    ]
    
    ;---try plain parse
        
    if (parse/all input-string [["{" | "["] thru "}" end]) [
        ;--reset staves block
        notes: copy []
        ;---catches text of form {foo bar } and [foo bar}
        append notes input-string
    ]
    
    ;print input-string
    if (parse/all (trim input-string) [other-rhythm-element]) [
        ;--reset staves block
        notes: copy []
        append notes input-string
    ]
    
    if parse/all (trim input-string) [bar-lines] [
        notes: copy []
        append notes input-string
    ]

    ;---whole line comments
    if input-string/1 = #"%" [
        notes: copy []
    
        append notes any [
            if (parse input-string ["%Bar " [0 3 number] to end]) [
               ignored: on
                ""
            ]
            if (parse input-string ["% Bar " [0 3 number] to end]) [
               ignored: on
                ""
            ]
            if (parse input-string ["%Line " [0 3 number] to end]) [
               ignored: on
                ""
            ]
            if (parse input-string ["% Line " [0 3 number] to end]) [
               ignored: on
                ""
            ]
            input-string    ;default
        ]
    ]
        
     either  ignored [
        none
     ] [
     
        if ("" = rejoin notes) and 
            ((trim input-string) <> "") 
             [
             print rejoin ["=== error parsing " input-string ]
        ]
        
        notes: right-pad-block notes 8
        
        :notes
    ]
]

right-pad-block: func [blk required-length] [

    ;---ensure we have at least n -teims staves
    while [required-length > length? blk] [
        append blk ""
    ]
    
    :blk
]





parse-tab-stream: func [tab-content /local result] [
    result: copy []
     
    foreach item parse/all tab-content "^/" [
          parse-line: parse-tab join item "  "
         
         if not none? parse-line [
            ;print mold parse-tab item
            ;---append item as a block
            append/only result  parse-tab join item "  "
         ]
    ]
    
    :result
]



is-header?: func [content] [
    
    result: false
    
    if content/1 = #"-" [result: true]
    if content/1 = #"{" [result: true]
    if content/1 = #"$" [result: true]
    
    return result
    
]

is-comment?: func [content] [
    return (content/1 = #"%")
]

parse-convert: func [input-tab-stream ] [
;--parses the current piece

    
    raw-stream: copy input-tab-stream
    
    ;---escape any tabs in the stream as \t instead.
    raw-stream: replace/all raw-stream tab "\t"
    
    tab-body: copy ""
    headers: copy ""
    header-finished: off
    
    foreach line parse/all raw-stream "^/" [
        
        case [
            is-header? line [
                append headers join line newline
            ]
            
            is-comment? line [
                either header-finished [
                    ;---comments in body section go into body
                    append tab-body join line newline
                ] [
                    ;---comments in header section go into header
                    append headers join line newline
                ]
            ]
            
            true [
                ;--default - this is a body element
                header-finished: on     
                
                append tab-body join  line newline
                
            ]
        ]
        
    
    ]
    
    
    if none? headers [ headers: ""]
    
    stave-blocks: parse-tab-stream  tab-body

    either (headers <> "") [
        header-block: parse/all headers "^/"
    ] [
        header-block: copy []
    ]
    
    
    ;---for debugging to see what we have now
    ;print mold stave-blocks
    ;foreach stave stave-blocks [
    ;    print mold stave
    ;]
        
    ;---return a block containing headers and staves
    result-block: copy []
    
    append/only result-block headers
    append/only result-block stave-blocks
    
    ;---return the result
    :result-block
]



