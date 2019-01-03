REBOL [
    title: "tests of various parsing routines"
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

test-tab-to-staves: does [

    do load %parse-tab-lib.r
    
        win-path: {..\Samples\Tab\Capirola_Weirdness.tab}
        rebol-path: join what-dir to-rebol-file win-path
         res: parse-convert read rebol-path
        
        probe res/1
        
        foreach item res/2 [ print mold item]

]



test-fronimo-to-tab: does [

      do load %parse-ft3-lib.r

    samples-folder:  {..\Samples\Ft3\}

    ;sample-file: {55_mrs_winters_jump\55_mrs_winters_jump.ft3}
    ;sample-file: {27_galliard\27_galliard.ft3}
    ;sample-file: {28_galliard_upon_bachelor\28_galliard_upon_bachelor.ft3}
    sample-file: {05_a_fancy\05_a_fancy.ft3}
    ;sample-file: {queen_elizabeths_galliard\queen_elizabeths_galliard.ft3}
    ;sample-file: {recercare_04\recercare_04.ft3}
    ;sample-file: {25_melancholy_galliard\25_melancholy_galliard.ft3}
    ;sample-file: {frogg_galliard\frogg_galliard.ft3}
    ;sample-file: {02 Piper's Pavan - Minus Lute 1-1.ft3}
    ;sample-file: {ec9faec4-2e64-443c-bb40-a46ba15c2454.tmp}

    if none? system/options/args [
        in-path: rejoin [ what-dir samples-folder sample-file]
        out-path: "d:\desktop\out3.txt"
        
        system/options/args: reduce [in-path out-path]
    ]
    
    arg-block: probe to-block system/options/args
    in-file: to-rebol-file arg-block/1
    out-file: to-rebol-file arg-block/2
    
    probe content: parse-fronimo-file to-rebol-file in-path
    write out-file content
    
]

test-fronimo-to-xml: does [

    ;===================configurable=============
    win-path: {..\Samples\WorkInProgress\02 Piper's Pavan - Minus Lute 1-1.ft3}
    out-path: {d:\desktop\out2.txt}
    
    system/options/args: reduce [win-path out-path]
    
    do %FronimoToXML.r
    
    print read to-rebol-file out-path
]

test-tab-to-xml: does [

    ;===================configurable sample data=============
    win-path: {..\Samples\Tab\dowland_willoughby_1.tab}
    win-path: {D:\desktop\programming\projects\LuteScribe\LuteScribe.Tests\TestData\NoBreakHeadersBody.tab}
    ;win-path: {d:\desktop\test2.tab}
    out-path: {..\Data\rebol-generated.lsml}
    rebol-path: join what-dir to-rebol-file win-path
    ;============================================

    system/options/args: reduce [enbase/base win-path 64]
    
    if (not none? system/options/args) [
        out-path: {..\Data\rebol-generated.lsml}
    ]
    
    do %TabToXML.r
    ;write (join what-dir to-rebol-file out-path) out

]




;---actual test call
;test-fronimo-to-xml     ;ok
test-fronimo-to-tab  ;ok
;test-tab-to-staves
;test-tab-to-xml

