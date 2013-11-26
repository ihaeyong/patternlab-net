
## About the .NET Version of Pattern Lab

The .NET version of Pattern Lab is, at its core, currently a copy of the PHP one, but I'll get there.

## Under Active Development

~~The .NET version of Pattern Lab is under active development by [@benedfit](https://twitter.com/benedfit).  Contributions welcome (once we get off the ground).~~

**I've currently shelved development, as I'm unsure of whether this is actually something that needs creating**

### Getting Started

You'll know when I know.

#### Roadmap
* Full Patternlab site support.  (This is the uber cool navigation found at [demo.pattern-lab.info](http://demo.pattern-lab.info)).
* More Documentation
* Tests

**THE FOLLOWING IS FROM THE PATTERNLAB-PHP PROJECT.  A LOT STILL APPLIES TO PATTERNLAB-DOTNET, BUT IT HAS NOT BEEN ADAPTED YET.  USE AT YOUR OWN PERIL**


===

## Demo

You can play with a demo of the front-end of the PHP version of Pattern Lab at [demo.pattern-lab.info](http://demo.pattern-lab.info).

## Getting Started

The PHP version of Pattern Lab should be relatively easy for anyone to get up and running. 

* [Requirements](https://github.com/pattern-lab/patternlab-php/wiki/Requirements)
* [Installing the PHP Version of Pattern Lab](https://github.com/pattern-lab/patternlab-php/wiki/Installing-the-PHP-Version-of-Pattern-Lab)
* [Generating the Pattern Lab Website for the First Time](https://github.com/pattern-lab/patternlab-php/wiki/Generating-the-Pattern-Lab-Website-for-the-First-Time)
* [Editing the Pattern Lab Website Source Files](https://github.com/pattern-lab/patternlab-php/wiki/Editing-the-Pattern-Lab-Website-Source-Files)
* [Using the Command-line Options](https://github.com/pattern-lab/patternlab-php/wiki/Using-the-Command-line-Options)

## Working with Patterns

Patterns are the core element of Pattern Lab. Understanding how they work is the key to getting the most out of the system. Patterns use [Mustache](http://mustache.github.io/) so please read [Mustache's docs](http://mustache.github.io/mustache.5.html) as well.

* [How Patterns Are Organized](https://github.com/pattern-lab/patternlab-php/wiki/How-Patterns-Are-Organized)
* [Adding New Patterns](https://github.com/pattern-lab/patternlab-php/wiki/Adding-New-Patterns)
* [Reorganizing Patterns](https://github.com/pattern-lab/patternlab-php/wiki/Reorganizing-Patterns)
* [Converting Old Patterns](https://github.com/pattern-lab/patternlab-php/wiki/Converting-Old-Patterns)
* ["Hiding" Patterns in the Navigation](https://github.com/pattern-lab/patternlab-php/wiki/Hiding-Patterns-in-the-Navigation)
* [Including One Pattern Within Another via Partials](https://github.com/pattern-lab/patternlab-php/wiki/Including-One-Pattern-Within-Another)
* [Linking Directly to a Pattern](https://github.com/pattern-lab/patternlab-php/wiki/Linking-Directly-to-a-Pattern)
* [Managing Assets for a Pattern: JavaScript, images, CSS, etc.](https://github.com/pattern-lab/patternlab-php/wiki/Managing-Assets-for-a-Pattern)
* [Modifying the Standard Header & Footer for Patterns](https://github.com/pattern-lab/patternlab-php/wiki/Modifying-the-Standard-Header-&-Footer-for-Patterns)

## Creating & Working With Dynamic Data for a Pattern

The PHP version of Pattern Lab utilizes Mustache as the template language for patterns. In addition to allowing for the [inclusion of one pattern within another](https://github.com/pattern-lab/patternlab-php/wiki/Including-One-Pattern-Within-Another) it also gives pattern developers the ability to include variables. This means that attributes like image sources can be centralized in one file for easy modification across one or more patterns. The PHP version of Pattern Lab uses a JSON file, `source/_data/data.json`, to centralize many of these attributes.

* [Introduction to JSON & Mustache Variables](http://github.com/pattern-lab/patternlab-php/wiki/Introduction-to-JSON-&-Mustache-Variables)
* [Overriding the Central `data.json` Values with Pattern-specific Values](https://github.com/pattern-lab/patternlab-php/wiki/Overriding-the-Central-%60data.json%60-Values-with-Pattern-specific-Values)
* [Linking to Patterns with Pattern Lab's Default `link` Variable](https://github.com/pattern-lab/patternlab-php/wiki/Linking-to-Patterns-with-Pattern-Lab's-Default-%60link%60-Variable)
* [Creating Lists with Pattern Lab's Default `listItems` Variable](https://github.com/pattern-lab/patternlab-php/wiki/Creating-Lists-with-Pattern-Lab's-Default-%60listItems%60-Variable)

## Using Pattern Lab's Advanced Features

By default, the Pattern Lab assets can be manually generated and the Pattern Lab site manually refreshed but who wants to waste time doing that? Here are some ways that the PHP version of Pattern Lab can make your development workflow a little smoother:

* [Watching for Changes and Auto-Regenerating Patterns](https://github.com/pattern-lab/patternlab-php/wiki/Watching-for-Changes-and-Auto-Regenerating-Patterns)
* [Auto-Reloading the Browser Window When Changes Are Made](https://github.com/pattern-lab/patternlab-php/wiki/Auto-Reloading-the-Browser-Window-When-Changes-Are-Made)
* [Multi-browser & Multi-device Testing with Page Follow](https://github.com/pattern-lab/patternlab-php/wiki/Multi-browser-&-Multi-device-Testing-with-Page-Follow)
