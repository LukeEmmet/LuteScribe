REBOL [
    title: "Fronimo to Tab format"
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


fronimo-to-tab: funct [input-path] [

    input-file-data: read input-path
    
    ;---auto-detect input type:
    ;---compressed file header of ft3 - gzip
    if (to-binary copy/part input-file-data 4) = #{1f 8b 08 00} [input-type: "ft3gz"]

    ;---uncompressed file header of ft3, typically looks like this
    ;if (to-binary copy/part input-file-data 8) = #{00 00 00 00 12 00 00 00} [input-type: "ft3"]
    ;if (to-binary copy/part input-file-data 8) = #{00 00 00 00 13 00 00 00} [input-type: "ft3"]
    ;if (to-binary copy/part input-file-data 8) = #{00 00 00 00 13 00 01 00} [input-type: "ft3"]
    ;if (to-binary copy/part input-file-data 8) = #{00 00 00 00 15 00 00 00} [input-type: "ft3"]
    if (to-binary copy/part input-file-data 4) = #{00 00 00 00} [input-type: "ft3"]

    ;---compressed file header of ft2
    if (to-binary copy/part input-file-data 4) = #{07 00 00 00} [input-type: "ft2"]
    if (to-binary copy/part input-file-data 4) = #{08 00 00 00} [input-type: "ft2"]


    switch  (lowercase input-type) [
        "ft2" [
            do load %parse-ft2-lib.r
            parse-result:  parse-fronimo-file input-path
        ]
        
        "ft3" [
            do load %parse-ft3-lib.r
            parse-result:  parse-fronimo-file input-path
        ]
        
        "ft3gz" [
            ;---default 
            make error! rejoin [ input-type " format is not supported - unzip (gzip) it first."]
        ]
    ]
]


