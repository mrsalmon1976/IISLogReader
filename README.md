IISLogReader
==================

# Overview 

IISLogReader is a simple, web-based application that can be used to analyse log files.  It's primary use is for inspecting average page response times, but it can be used to look for errors and various other statistics.  The application runs as a [NancyFx](http://nancyfx.org/) dashboard i.e. it can be installed as a service and accessed with a web browser.

[![Build status](https://ci.appveyor.com/api/projects/status/4pcqe0vfp3e8p3j0?svg=true)](https://ci.appveyor.com/project/mrsalmon1976/iislogreader)

# Features

- Administrative and read-only users
- Multiple project support
- Multiple IIS log files per project
- Load times, grouped by URL
- Ignore specific URLs using regular expressions
- Group specific URLs using regular expressions, allowing for the group of URLs with dynamic routes or querystring parameters (see URI Aggregates below)
- View server errors

## URI Aggregates

URI Aggregates allows you to transform URI stems containing dynamic data into a single consolidated URI stem that gets reported as a single item. By default, the following URI stems will be reported separately:

```
/products/123/test
/products/456/test
```

However, you may want these aggregated into a single item so the timings of these get consolidated into one average. By adding the following regular expression:

```
^/products/[0-9]+/test$
```

with aggregate target:

```
/products/{id}/test
```

these two URLs would be reported as one, and averages calculated on the single aggregate target.

# Screenshots

## Project Load Times

![Project load times screenshot](/docs/screenshots/project_load_times.png?raw=true "Project load times screenshot")

