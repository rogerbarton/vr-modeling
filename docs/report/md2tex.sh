#!/usr/bin/bash

# Converts the md files to tex files and filters out unwanted strings
# Uses pandoc, ignores rst parts

out=out.tex
rm $out

i=0
for file in "$@"; do
  tex=${file%.md}.tex
  echo "$file -> $tex"
  
  # Preprocess md before converting
  md=${file%.md}.tmp.md
  cp $file $md

  sed -i -r -e 's/^\.\.\s+\w*::.*//g' $md    # remove rst comments or notes
  sed -i -r -e 's/:c\w\w?:\w+?://g' $md       # remove :cs:func: or similar
  sed -i -r -e 's/`([^`]+)`_/\1/g' $md       # remove rst references

  pandoc -s $md -o $tex
  rm $md
  
  # 'trim' output tex file by lines, so we only get the content
  startln=$(grep -n "begin{document}" $tex | cut -f1 -d:)
#  echo "startln: ${startln}"
  let 'startln++'
  tail -n +${startln} $tex > $tex.tmp

  endln=$(grep -n "end{document}" $tex.tmp | cut -f1 -d:)
#  echo "endln  : ${endln}"
  let 'endln--'
  head -n ${endln} $tex.tmp > $tex
  rm $tex.tmp

  # Find and replace parts
  sed -i -e "s/\\\\section/\\\\setcounter{chapter}{$i}\\\\chapter/" $tex
  let 'i++'

  sed -i -e 's/\\subsection/\\section/' $tex
  sed -i -r -e 's/:cite:\\texttt/\\cite/g' $tex

  # filter out other unwanted parts
  sed -i -r -e 's/\\tightlist//g' $tex
  sed -i -r -e 's/\\begin\{verbatim\}//' $tex
  sed -i -r -e 's/\\end\{verbatim\}//' $tex

  cat $tex >> $out
  rm $tex
done