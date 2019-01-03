REBOL [
    
    title: {Staves to XML}
    description: {Converts blocks of header and body elements to XML that can be loaded by LuteScribe}

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


html-escape: funct [input-string] [
    out: copy input-string
    replace/all out "&" "&amp;"
    replace/all out "<" "&lt;"
    replace/all out ">" "&gt;"
    replace/all out {"} "&quot;"
    
     out
]
    
staves-to-xml: funct [headers body] [

    out: copy ""
    start: off
    new-stave: off
    
    append out rejoin [
        {<Piece>} newline
        {<Headers>} newline
    ]

    ;---headers is a long string delimted by newlines - split up
    foreach  item (parse/all headers "^/") [
      if ("" < trim item) [
        append out rejoin [
            {     <Header>} newline
            {       <Content>} (html-escape item) {</Content>} newline
            {      </Header>} newline
        ]
      ]
    ]
    
    append out rejoin [
        {</Headers>} newline
        {<Staves>} newline
        {  <Stave>} newline
        {    <Chords>} newline
    ]

    foreach item body [
        
        either  ((trim item/1) = "") [
            ; temporary check for actual content having been generated so far...
            ; first line seems to be empty at present (may be indicated empty prelude?)
            ; fix this 
            if start and (not new-stave) [
                append out rejoin [
                    {    </Chords>} newline
                    {  </Stave>} newline
                    {  <Stave>} newline
                    {    <Chords>} newline
                ]
                
            ]
            new-stave: on
        ] [
            start: on
            new-stave: off
            
            append out rejoin [
                {      <Chord>} newline
                {        <Flag>} (html-escape item/1) {</Flag>} newline
                {        <C1>} (html-escape  item/2) {</C1>} newline
                {        <C2>} (html-escape item/3) {</C2>} newline
                {        <C3>} (html-escape item/4) {</C3>} newline
                {        <C4>} (html-escape item/5) {</C4>} newline
                {        <C5>} (html-escape item/6) {</C5>} newline
                {        <C6>} (html-escape item/7) {</C6>} newline
                {        <C7>} (html-escape item/8) {</C7>} newline
                {      </Chord>}  newline
            ]
        ]
    ]

    append out rejoin [
                {    </Chords>} newline
                {  </Stave>} newline
                {</Staves>} newline
                {</Piece>} newline
    ]


    out

]