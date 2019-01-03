REBOL [
    title: " Fronimo to XML"
    description: "Converts FRonimo Ft2/ft3 (not gzipped ft3) to XML"

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

do load %FronimoToTab.r
do load %StavesToXML.r
do load %parse-tab-lib.r

arg-block: to-block system/options/args
rebol-path:  to-rebol-file arg-block/1
out-path: to-rebol-file arg-block/2


input-file-data: read rebol-path
input-file-path: rebol-path


    
either (error? err-result: try [res: parse-convert fronimo-to-tab rebol-path]) [
    res: disarm err-result
    print join "Error in converting Fronimo to XML: " mold res
    quit/return 10
] [
    headers: res/1
    body: res/2
    
    ;---currenlty assumes a single piece only
    out: copy ""
    append out  join {<?xml version="1.0" encoding="utf-8"?>} newline
    append out join  {<TabModel Vesion="1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">} newline
    append out  join  {<Pieces>} newline

    append out staves-to-xml headers body
    
    append out join {</Pieces>}  newline
    append out join {</TabModel>} newline

    write out-path out
;    write (join what-dir to-rebol-file out-path) out
    quit/return 0   ;success
]