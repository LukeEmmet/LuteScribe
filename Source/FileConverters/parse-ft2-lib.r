REBOL [
	title: {incomplete parser for fronimo .ft2}
	description: {a partial parser for fronimo .ft3 format
        which was abandoned in favour of ft3 parser which is 
        more reliable. Very few files seem to be in ft2 format.
    }
	
	nice-to-have: {
		automatic extraction of file resource from within .ft3 file 
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


debug-on?: false
output-stave-comments: false
output-bar-numbers: false
start-bar-data: 10	;---is there a way to determine this?


;_________SHARED FUNCTIONS____________________

debug-print: func [data] [
	if debug-on? [
		probe data
	]
]

is-bar-maker?: func [data] [
	not none? any [
		(#{040D0000FF} = to-binary data) 
		(#{040D5000FF} = to-binary data)
	]

]


;---function for parsing a bar data structure
extract-bar: func [data state-buffer /local result] [
	
	;---if buffer defined by previous iteration, use it
	if (length? state-buffer) > 0 [
		insert head data state-buffer
		state-buffer: copy ""
	]
	
	
	if #{00fdff} = to-binary data [
		debug-print "===using TMP-buffer"
		state-buffer: data
		data: tail data
	]
	
	
	char-count: 0
	result: copy []
	
	;debug-print mold (to-binary at data ((length? data) - 100))
	
	debug-print join "bar length: " (length? data)
	
	debug-print mold to-binary data
	
	;---jump over bar header if it is long enough
	either is-ft2? [
		either 15 < length? data [
			debug-print "jumping bar header..."
			data: at data 16
		] [
			;---not a real bar, so advance to just before the end and see
			;---if we find the end of bar marker
			
			;debug-print "not real bar"
			data: at data ((length? data) - 4)
			;probe to-binary data
			
			if is-bar-maker? data [
				;debug-print "got here"
				append/only output-block ["b"]
			]
			
			data: tail data
		]
	] [
		either 23 < length? data [
			data: at data 24
		] [
			data: tail data
		]
	]
	
	;debug-print "__"
	;if bar-counter = 14 [probe to-binary data]
	
	;if data/1 = #{00} [data: next data]
	
	;probe to-binary data
	
	while [not tail? data] [
	
		
		;---skip over any records that start with a high number
		;---possibly indicating fingering. Ornamentation is done by
		;---modifying the stave position elements themselves
		
		;probe to-integer data/1
		
		if (
		     (data/1 <> #"ÿ") and
		     (
			(is-ft2? and ((to-integer data/1) > 10)) 
			or ((to-integer data/1) > 200)
		     )
		   ) [
			;debug-print length? data
			debug-print rejoin ["^/possible fingering: " mold data]
			;probe to-binary data
			
			;probe to-binary copy/part data 2
			;probe to-binary data
			data: skip data 2
		]
		
		if bar-counter = 20 [
			debug-print to-binary copy/part data 10
		]
		
		;debug-print rejoin [
		;	(length? data) " " 
		;	(to-binary copy/part (at data 3) 4)
		;]
		
		;---FT3 - if bar is of the form #{XX48e0edbf} then this indicates
		;---the remaining elements within a bar (perhaps it is the mensural
		;---notation?) Anyway if we get to it we can skip to the end if
		;---we are only extacting the tab.
		if #{48e0edbf} = (to-binary copy/part (at data  3 ) 4)  [
			;debug-print "XXXXXX"
			append/only output-block ["b"]
			break
		]
		
		if #{04003db5b0bf} = (to-binary copy/part data 6) [
			debug-print "junk type 2"
			append/only output-block ["b"]
			break
		
		]

		;---ft2
		if 23 = length? data [
			;probe to-binary data
			
			;---detect end of bar markers...
			if (
				;(#{000000000000010000000000000003800000040D0000FF} = to-binary data) 
				;or
				;(#{000000000000000000000000000003800000040D0000FF} = to-binary data)
				;or
				;(#{000000000000000000000000000003808005040D0000FF} = to-binary data)
				;or
				is-bar-maker? at data 19
			  ) [
				;debug-print "####end of bar"
				append/only output-block ["b"]
				break
			]
			
		]
		;debug-print "^/--- stave element"
		
		;debug-print reform [
		;	(mold to-binary data/1)
		;	(mold to-binary data/2)
		;	(mold to-binary data/3)
		;	(mold to-binary data/4)
		;	(mold to-binary data/5)
		;	(mold to-binary data/6)
		;]
	
		stave-output: copy [" " " " " " " " " " " " " " " " " "]
		
		if (error? try [
			num-records: to-integer data/1
			
			
			;rhythym-output: copy [" " " " " " " " " " " " " " " " " "]
			
			
			;---**need to convert these into number of flags etc...
			stave-output/1: to-string pick "Ww01234" (1 + to-integer data/3) 
			
			;debug-print mold to-binary data/4
			
			if  16 = (to-integer data/4) [
				;debug-print "dot!"
				append stave-output/1 "."
			]
			
			if 2 = (to-integer data/4) [
				insert head stave-output/1 "#"
			]
			
			if (4 = to-integer data/4) or (8 = to-integer data/4) [
				stave-output/1: "x"
			]
			
			;---skip over stave header to get to the real data
			data: skip data 6
			
			cur-record: 1
			;debug-print join "num-records: " num-records
			
			while [cur-record < num-records ] [
				;debug-print join "--- course-record " cur-record
				;debug-print join ": " next data
				
				;debug-print join "cur-record: " cur-record
				
				cur-letter-num:  to-integer copy to-string data/2
				;probe cur-letter-num
				
				
				either (8 = to-integer data/1) [
					;---bourdons
					cur-letter: to-string pick ["a" "/a" "//a" "///a" "4" "5" "6" "7"] (cur-letter-num + 1)
				] [
					;---other courses...
					cur-letter: to-string pick "abcdefghjklmnopq" (cur-letter-num + 1)
				]
				
				;probe cur-letter-num
				
				poke stave-output ((to-integer data/1) ) (cur-letter)
				
				;debug-print reform [
				;	(mold to-integer data/1)
				;	(mold to-string data/2)
				;	(mold to-binary data/3)
				;	(mold to-binary data/4)
				;	(mold to-binary data/5)
				;]				
				
				data: skip data 5
				cur-record: cur-record + 1

			]
			
			;probe stave-output
			;result: copy/deep stave-output
			append/only output-block :stave-output
			
			
		]) [
			;---is not a bar
			;debug-print "===invalid bar"
			data: next data
			;result: copy []
			
		]
			;probe result
		
	]
	
	;probe result
	;probe :stave-output
	;:stave-output 

]

 




	

parse-fronimo-binary-content: func [data output-block] [

	marker: to-string #{ff}

	counter: 0
	bar-counter: 0
	 
	state-buffer: copy ""

	 parse/all data [
	
		;to barline
		;thru "New" (debug-print "OK") to end
		
		
		copy header to marker
		
		any [
			[
				copy bar thru marker
				
				(
					counter: counter + 1
					
					
					either counter >= start-bar-data [
						bar-counter: bar-counter + 1
						;debug-print reform ["record: " counter ]
						;debug-print reform ["bar: " bar]
						;append/only 
						
						if 0 = remainder bar-counter 6 [
							;---every 8 bars create new system
							append/only output-block [""]
							append/only output-block [""]
							append/only output-block ["b"]
						]
						
						;debug-print "___BAR__"
						
						;probe to-binary bar
						;---sometimes we get empty bars, maifest as bars with #{ff}
						;---as sole content - this is not a problem
						;---as bar parser can deal with it.
						debug-print rejoin ["bar " bar-counter ": " mold bar]
						
						if output-bar-numbers [
							append/only output-block to-block mold join "%bar " bar-counter
						]
	
						;if  bar <> "ÿ" [
						;	debug-print join "extracting bar " counter
							extract-bar bar state-buffer
						;]
					] [
						
						if "^A^@^F^@CPiece" = copy/part bar 10 [
							;debug-print join "TITLE!" at bar 20
							bar: at bar 20
							
							if parse/all bar [copy title to "^A" to end] [
							
								title: replace/all title "^@" ""
								title: replace/all title "^L" "/"		;---convert newlines to forward slashes...
								title: trim title
                                
                                        title: replace title #{08} " "      ;may indicate acute accent or newline in title?, anyway causes problem for  resultant XML
								;debug-print mold title
								;---add a string to the header of the title
								;debug-print mold title
								append/only header-block  to-block mold rejoin ["{" title "}"]
								;debug-print title
							]
							
							;debug-print mold end-title
						]
						
						debug-print rejoin [
							"header: (length = " (length? bar) ") " (mold bar)
							newline
							(to-binary bar)
						]
					]
				)
			]
			
	
		]
		
		copy rest to end
		(extract-bar rest state-buffer)
		
		;	["New" (debug-print now) ]
			;"ÿ" (debug-print "OK")]
		;]
		
		end
	]
	

]


;probe output-block
;probe header-block

;_____________________________________________________________________
;---start outputting to the file


parse-fronimo-file: func [
	file-name
	/local
	
	result
	] [
	
	result: copy  ""
		
	;---decide whether file is ft2 or ft3 by looking at file extension
	is-ft2?: "ft2" = lowercase last parse/all file-name "." 
	
	debug-print join "===FILE TYPE: " (either is-ft2? ["ft2"] ["ft3"])
	
	
	;--try this?
	;---data: replace/all data (to-string #{ffff}) (to-string #{0000})
	
	parse-bar-block: copy []
	output-block: copy []
	header-block: copy []
	
	append/only header-block to-block mold "%Converted by Marmaladefoo.com Fronimo to Tab converter"
	
	;---main parsing here
	parse-fronimo-binary-content (read/binary file-name) output-block
	
	
	;---write headers
	foreach item header-block [
		append result trim rejoin item
		append result  newline
	]
	
	;---start main content
	append result "^/^/"
	append result "b^/"
	
	;print "Content-Type: text/html^/"
	
	;---start outputting content
	foreach item output-block [
		;probe item
		
		;either ((pick (copy trim item) 1) = #"%") [
		;	if output-stave-comments [
		;		append result trim rejoin item
		;		append result  newline
		;	]
		;] [
			append result trim rejoin item
			append result  newline
		;]
		
		
	]
	
	;---write end of file to close
	append result "e"
	
	:result
]



