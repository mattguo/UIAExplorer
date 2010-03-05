#!/bin/sh

# path of accerciser/pixmaps
SRCDIR='.'
# path of uia-explorer's icon path
DSTDIR='Icons'

if [ "x$1" != "x" ]; then
	SRCDIR=$1
fi
if [ "x$2" != "x" ]; then
        DSTDIR=$2
fi
if [ ! -d $DSTDIR ]; then
	mkdir $DSTDIR
fi

ci() {
	if [ "x$2" == "x" ]; then
		cp $SRCDIR/$1.png $DSTDIR/$1.png
	else
		cp $SRCDIR/$1.png $DSTDIR/$2.png
	fi
}

ci pushbutton button
ci calendar
ci checkbox
ci combobox
ci treetable datagrid
ci tablecell dataitem
ci text document
ci entry edit
ci filler group
ci columnheader header
ci row headeritem
ci link hyperlink
ci image
ci list
ci listitem
ci menu
ci menubar
ci menuitem
ci filler pane
ci progressbar
ci radiobutton
ci scrollbar
ci separator
ci slider
ci spinbutton spinner
#???
ci combobox splitbutton
ci statusbar
ci pagetablist tab
ci pagetab tabitem
ci table
ci label text
ci filler thumb
ci filler titlebar
ci toolbar
ci tooltip
ci tree
#???
ci listitem treeitem
ci window
ci drawingarea custom
ci invalid
