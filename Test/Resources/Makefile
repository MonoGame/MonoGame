open: output
	open obj/results.html

output: tests.xsl test_results.xml
	mkdir -p obj
	xsltproc -o obj/results.html tests.xsl test_results.xml

clean:
	rm -rf obj
