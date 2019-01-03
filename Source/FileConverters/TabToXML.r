REBOL [
    title: " Tab to XML"
    description: "Converts Wayne Cripps formatted Tab file to XML"

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

do load %parse-tab-lib.r
do load %StavesToXML.r

arg-block:  system/options/args
rebol-path:  to-rebol-file (to-string debase/base arg-block/1 64)
    

get-sections: funct [lines] [
    sections: []
    section: copy ""
    line-count: 0
    
    foreach line lines [
        either line = "%=== new section ===" [
            if line-count > 0 [append sections section]
            line-count: 0
            section: copy ""
        ] [
            append section line
            append section newline
            line-count: line-count + 1
        ]
    ]

    if line-count > 0 [append sections section]
    return sections
]




;--output the content...
prin join {<?xml version="1.0" encoding="utf-8"?>} newline
prin  join  {<TabModel Version="1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">} newline
prin  join  {<Pieces>} newline

foreach section  get-sections read/lines rebol-path [

    res: parse-convert section

    headers: res/1
    body:  res/2

    out: staves-to-xml headers body

    print out

]

prin join {</Pieces>}  newline
prin join {</TabModel>} newline

