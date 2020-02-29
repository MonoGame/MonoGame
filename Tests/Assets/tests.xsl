<?xml version="1.0"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" />

    <xsl:template match="/">
        <xsl:variable name="tests.root" select="test-results" />
<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html>
</xsl:text>
<html>
    <head>
        <title><xsl:value-of select="$tests.root/@name" /></title>
        <style>
        .plus {
            display: inline-block;
            width: 16px;
            height: 16px;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABmJLR0QAAAAAAAD5Q7t/AAAACXBIWXMAAA3XAAAN1wFCKJt4AAAAB3RJTUUH1gELEAARlER4oQAAANBJREFUOMvNkb0OwWAUhh/SxWC3cgfSuoHGYDZbO3RxB0TSxGQXsVoNJoOEG+hnN1osVmnip98xEIr6iwTvdvKdvPme58Cvk7j3YDk9HzCPo/K7FStuL/mg3PRcG8+1iRS9VfBSPi4w7jDHOZHIeHJiRJlb1SJbLSSAbXje91wbERA5WK93xubNDwBW65D5MkAEtBa0QLARwlDYaUFryGVS8QiAanQmFwjlUgGAwci/JlJP5VhOT4bThTT7M7ni//IZ89n0a2eMiaq1T7YVf5s9VHJLC4uyd44AAAAASUVORK5CYII=);
        }
        .minus {
            display: inline-block;
            width: 16px;
            height: 16px;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABmJLR0QAAAAAAAD5Q7t/AAAACXBIWXMAAA3XAAAN1wFCKJt4AAAAB3RJTUUH1gELDzsiAFwSwgAAAIRJREFUOMvtkbEJg1AQhr8HadzAOivoCBkjrW1GCBkgC4itc8QFdIjUtiJE8+5PIcqzszOFHxwcB/93HAcH++PmJs3KGkg25pq6uKYAp2CYPG8XRhMOGL1WCWkqB9zz17IoFNB9PO+2RwIzYYJ+EN6LrwkzOMfRShwKmkdebT7h+P4/8QOsUy3bRuSA2AAAAABJRU5ErkJggg==);
        }
        .check {
            display: inline-block;
            width: 16px;
            height: 16px;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAiNJREFUeNpi/P//PwMlgIlUDZxTGFnINoBrHiO3wGOhk0JxAtvhgiAvEIM5ZzNwi1cIXNh7fdf/2Hlh/5l8GbaAxBmJCQPO6Yzc/A8Fji1KWKa38MQchue/XjAI/OJn2LBpx2a4AaLL2IQZ/jMqvY7+eRpZM98CRm72G3zHFqcu01t8bS7DrjdbGZi/szL83sd67+fXL8bgMOCYwiii9kr/lsE7yyPcCxgdYZql1nHz8DwQPLY4Z6neyhcLGU6zHmHgYeVleLf1+50vnz/ofNn++wOLWBuXmBOXy7VCv0KhL3+/MHzc+mGrxBoON24unlOsN3iP9iT36W1+u5bhOutlBq6fvAzXVj68y/jvn+7Pnf9/gGPh09k/fbKqkkL7v+xi+Mr6hSErIJNT8IbMDuYTvHeaEpr0Tv08wnCf4yYD209OhuvLH95jlvqr+3M7RDMIgMOAM4Bjj0aQrK2+mT6bEa8pAysjGwM/qyDD9b+XGO4y3GT4/P4rw/6ek/fYzH/pvEv7+x05jMCJ4vuGHy7MAUy7GQQY7P5b/GFTY9NkeMf8muE183OGv58YGI5MO3dPIVpE+4rT4x/oMQRPVX83/HNlDWXaK8AnZC1oL8jOycjJ8O8jA8OB7hP3LPN0dbapHv2BNY7REwx7BMvu8NN+Pxvflf9XKlG4U/QthQNfAsMqyBvPtU+5Qf7snP8TOAmlUJwS6T8iWYlJ4oyUZmeAAAMArHtnGnCDAuYAAAAASUVORK5CYII=);
        }
        .error {
            display: inline-block;
            width: 16px;
            height: 16px;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAgBJREFUeNqcU01rE1EUPRPHzINk6gRSx48IjUgNtAtBqaZgzErqquDahcsU9B/4P+zOLNS/oJtg6KoBF0YY0o2NjZYmsaTTMAkzzWTGe8dRpjGk4oXD8O6ce+7Hu0/ChL0G7tLnKaFIyIbuJqFKKD8BalG+NBH8Umha6UahAD2XQ1JLAe4IVqeD9s4OvtRqsC1rk0Q2/hKg4HcL+fzarfV19La20K/X4XS7gO9DSaehLi5CW1rG5+oH7DUa70nk0R8BzrywslJaLj7A9zdvYe/vY5oJErpcuA/jUx2t3d2gEol7Fqq6/fDZc7TKr2AfHGCWiVQKV/N5VCoV2I5z79xj4MXN1dU7sdYejg0DZ5lr24h5HkQigUPTdGM87Yv6JfQbjTODfcKYcNxuY14IdhVlvqpEXEa71wtIt31/psi2JGE4HCLzi5eVA+XBAP9q44kzCzStTjcXp56cfh8fKYMflsvwQiByFoqCwclJsGA8g2qHri2pacHP8RT89rvh98LcHH5YFgtUWaD8lQRUVcV5GswsAYYgTpqusnl0xALlGO+2PRptGiRyJZOBQgQvEuRFMnPwdeIYtNqO6/Ii1U6t8jVNW1vSdRySuknzGDpO0Df3rFHZOmXm4G+meXqVo49JkeVSlojzySQS8Xjg54Fxz1x2mHlj6mv8n+f8U4ABAHfN3DSReMR0AAAAAElFTkSuQmCC);
        }
        .ignored {
            display: inline-block;
            width: 16px;
            height: 16px;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAtNJREFUeNpsU0trE1EUPjP3ziSZyat5Nba2tcU+LKXYbFqwUhFFiguhuHJnwZ3gLxBXLgQX7gVdCAqK4MaFCxdqW12ILfigLcRaadLa5tE8JpPM63rupGlLdZiPO9w533e+c+65Avz7RAiVRvxtiXNUCQ84DERLL//SS9vvLaOxiP9zCLsVLBwiEkR/pKPvZnxo4mqg+3SMqXGxUrfB1grM2PxRKqc/valkVh84zOFCxlGB4WT/2P2es9cuyl0paoAEps2gpFvuSjGpmFt2dj8/X8qvLNxijH3kTsgeOR7rGrjTe352hvaMU4OJYKN3v4eAinBdgAhMTQiBWGcSKtnjenFrDnlFkVuXZM9EcmRqRu5OEROJzh4mByMwd3sSFBnzMHw5ogNCdOTClC8YmUaujwuEgonuy6GT40GTUUDePsKK5NqjosD53AMIggBKT8oTaO+9glsxLhBUIx0pMZAUuG2sDclN1M1ms/k+lxBRiCCoGgU13j2Iv8JcwEuVQJtDPK5tHsyz81VrHAggzyVziJSCrIb8+BmiPIA5tu04DjjQrLPZA8DmWQcCPPOeAMG0gmNzkwZ3oDWqxQwY1X3rLQfpPzWYfbiI5tkhMgoxA8PzeQyrcoGitrMx38it2XwqLDxz03LAsB04EffBoxtj7lHyrByUYDfKm6yytfYFuTt8DgyjVtY8Xu8lz7HhsGZLgHy3FC8e34flPPzO17H7ABIhoBALikuvdzNf393Fyr+1BqlkVArM6/OdkaJ9ksWnGhnbpQas7eggYGYZySp1oJGeM9cXXj7WSvmnyCu3BAyzUUsbxQ2cDGNUDSe8gqyAiCSZiqBIePbWLtRW3urr8y+eFbI/7yFn4+hdcEdaVgLToc6h6/7OU6NyOKkSQgRHy+nVzPJqYf37k1q58Arjsq0bKfznOsuIdkJor+xTujCEmvVa1rLMNO5vIvTDwX8FGABpnleu8SWSbAAAAABJRU5ErkJggg==);
        }
        .warning {
            display: inline-block;
            width: 16px;
            height: 16px;
            background-image: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAjtJREFUeNqUk89rU0EQx7+zu+/XJnlJXpLX5iX2RzC1TasVhKJoQRGEevLUSwLe9CIexN6K4kXxUvVQEW8evHmx/gNePChqyUUQ1IJWCnqwJU3TNE3WbVChmEgdGGaYHT4z7H6XlFLoZOXzNARmPCPVrDZU69yRR+pzpz6GLsZM+ThzbCrfO3F23BTmg659HaeX6Exo4PCom/Ip2uMzOyicLBdpYm8AIuKR6HwqP+aQ44HZUaQOHLTJce7vCfC2iOnIwHhgWQwsMwoWDEM6BoWyIyOLJZr6J+D5KRJWOHrXC9KShAkWD8CSOUAIJPv6JHecedwg1hUQy+BCOJt3TaqDpYcwM/e07Tw9DJs3EO7N+YsfUewIeHORJJORm15CSrIkWDSFpGvAj+lNvAzItJDw3RAznLkPl8n6C8BquBrzfcNobYDvK0Bt1xDEDQSeAdXc1LUxWKjBTSSc6iou7QK8myZPmPZMPAJJEQ8UcoHtDfQnDeR6rHZObgIkODxzLaSld+1lidw/gIaN6xHXEkLVwPsLulAF6hVMDkscHeTA+jJQ/aq3GILRqsCVEA5htv3q5SKyiuN9LmtJM6sbDp3+LQjwE3faWfPFFWBH8qqFrdcL2FpZwqcVbDQV9gtw3I6FYXBV17c9iNbmmtaSXkz78pNSG6Tq6zoqzWiB941AfFtCPAz+o4JbQp9Mhm0YO5O2Xi3sEkn8V6x3UKC0Ya1WcVzozR5++Y5ZHS38jxE2mcI96vad92o/BRgAj+SRf7WThPMAAAAASUVORK5CYII=);
        }

        body {
            font: 11pt helvetica, arial, sans-serif;
            margin: 0;
        }

        h1 {
            margin: 0.5ex 0;
        }

        div#header {
            background-color: #eee;
            border-bottom: 1px solid #ccc;
            padding: 1ex;
            padding-bottom: 0;
            margin: 0;
        }

        #summary table th {
            text-align: center;
            color: #999;
            padding: 0.1ex 1ex;
        }

        #summary table td {
            text-align: center;
            padding: 0.1ex 1ex;
        }

        div#details {
            padding: 1em;
        }

        #details blockquote {
            margin-top: 0.25em;
        }

        .progress-bar {
            /* I'm not happy with the amount of cajoling needed here.
            The goal is simply to have the progress bar be vertically
            aligned to the middle of the line */

            font-size: 1pt;
            position: relative;
            display: inline-block;
            height: 5pt;
            width: 25pt;
            border-bottom: 6pt solid transparent;
            vertical-align: text-bottom;
        }

        .progress-bar-big {
            width: 100%;
            height: 14pt;
            border-bottom: 0;
        }

        .progress-bar .passed-progress {
            display: inline-block;
            height: 100%;
            background-color: #74b443;
        }

        .progress-bar .ignored-progress {
            display: inline-block;
            height: 100%;
            background-color: #fbd829;
        }

        .progress-bar .failed-progress {
            display: inline-block;
            height: 100%;
            background-color: #ec2d21;
        }

        .test-name {
            margin-left: 1em;
        }

        .failure-info {
            border: 1px solid #ccc;
            background-color: #ddd;
            padding-left: 2em;
            margin: 0.5em;
        }

        .failure-message {

        }

        .stacktrace {
            overflow: scroll;
        }

        </style>
    </head>
    <body>
        <!-- Debug display of the embedded icons
        <div class="plus" />
        <div class="minus" />
        <div class="warning" />
        <div class="error" />
        <div class="check" />
        -->
        <xsl:variable name="tests.total"
            select="$tests.root/@total" />
        <xsl:variable name="tests.errors"
            select="$tests.root/@errors" />
        <xsl:variable name="tests.failures"
            select="$tests.root/@failures" />
        <xsl:variable name="tests.ignored"
            select="$tests.root/@ignored" />
        <xsl:variable name="tests.skipped"
            select="$tests.root/@skipped" />
        <xsl:variable name="tests.inconclusive"
            select="$tests.root/@inconclusive" />
        <xsl:variable name="tests.passed"
            select="$tests.total - $tests.errors - $tests.failures" />

        <xsl:variable name="tests.grandtotal"
            select="$tests.total + $tests.ignored + $tests.skipped" />


        <div id="report">
            <script>
                function toggleDiv(imgId, divId)
                {
                    eDiv = document.getElementById(divId);
                    eImg = document.getElementById(imgId);

                    if (eDiv.style.display == "none")
                    {
                        eDiv.style.display = "block";
                        eImg.className = "minus";
                    }
                    else
                    {
                        eDiv.style.display = "none";
                        eImg.className = "plus";
                    }
                }
            </script>
            <div id="header">
                <h1><xsl:value-of select="$tests.root/@name" /></h1>
                <div id="summary">
                    <table>
                        <tbody>
                            <!--<tr>
                                <th>Assemblies tested:</th>
                                <td><xsl:value-of select="count($tests.root)"/></td>
                            </tr>-->
                            <tr>
                                <th>executed</th>
                                <th>pass</th>
                                <xsl:if test="$tests.failures > 0"><th>fail</th></xsl:if>
                                <xsl:if test="$tests.errors > 0"><th>error</th></xsl:if>
                                <xsl:if test="$tests.inconclusive > 0"><th>inconclusive</th></xsl:if>
                                <xsl:if test="$tests.ignored > 0"><th>ignored</th></xsl:if>
                                <xsl:if test="$tests.skipped > 0"><th>skipped</th></xsl:if>
                            </tr>
                            <tr>
                                <td><xsl:value-of select="$tests.total"/></td>
                                <td><xsl:value-of select="$tests.passed"/></td>
                                <xsl:if test="$tests.failures > 0">
                                    <td><xsl:value-of select="$tests.failures"/></td>
                                </xsl:if>
                                <xsl:if test="$tests.errors > 0">
                                    <td><xsl:value-of select="$tests.errors"/></td>
                                </xsl:if>
                                <xsl:if test="$tests.inconclusive > 0">
                                    <td><xsl:value-of select="$tests.inconclusive"/></td>
                                </xsl:if>
                                <xsl:if test="$tests.ignored > 0">
                                    <td><xsl:value-of select="$tests.ignored"/></td>
                                </xsl:if>
                                <xsl:if test="$tests.skipped > 0">
                                    <td><xsl:value-of select="$tests.skipped" /></td>
                                </xsl:if>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div class="progress-bar progress-bar-big">
                <xsl:call-template name="progressSegment">
                    <xsl:with-param name="value" select="$tests.passed" />
                    <xsl:with-param name="total" select="$tests.grandtotal" />
                    <xsl:with-param name="class">passed-progress</xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="progressSegment">
                    <xsl:with-param name="value" select="$tests.ignored + $tests.skipped" />
                    <xsl:with-param name="total" select="$tests.grandtotal" />
                    <xsl:with-param name="class">ignored-progress</xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="progressSegment">
                    <xsl:with-param name="value" select="$tests.errors + $tests.failures" />
                    <xsl:with-param name="total" select="$tests.grandtotal" />
                    <xsl:with-param name="class">failed-progress</xsl:with-param>
                </xsl:call-template>
            </div>
            <div id="details">
                <xsl:apply-templates select="$tests.root" />
            </div>
        </div>
    </body>
</html>
    </xsl:template>

    <xsl:template match="test-results">
        <xsl:variable name="test.suite.id" select="generate-id()" />
        <xsl:variable name="test.suite.name" select="./@name"/>
        <xsl:variable name="failure.count" select="count(.//results//test-case[@success='False'])" />
        <xsl:variable name="ignored.count" select="count(.//results//test-case[@executed='False'])" />

        <!-- Suite status icon -->
        <xsl:choose>
            <xsl:when test="$failure.count > 0">
                <div class="error">
                    <xsl:attribute name="title">Failed tests: <xsl:value-of select="$failure.count" /></xsl:attribute>
                </div>
            </xsl:when>
            <xsl:when test="$ignored.count > 0">
                <div class="ignored">
                    <xsl:attribute name="title">Ignored tests: <xsl:value-of select="$ignored.count" /></xsl:attribute>
                </div>
            </xsl:when>
            <xsl:otherwise>
                <div class="check" />
            </xsl:otherwise>
        </xsl:choose>

        <!-- Suite expand/collapse -->
        <div class="minus">
            <xsl:attribute name="id">img<xsl:value-of select="$test.suite.id"/></xsl:attribute>
            <xsl:attribute name="onclick">javascript:toggleDiv('img<xsl:value-of select="$test.suite.id"/>', 'divDetails<xsl:value-of select="$test.suite.id"/>');</xsl:attribute>
        </div>

        <span class="suite-name">
            <xsl:value-of select="$test.suite.name"/>
        </span>

        <!-- Suite details -->
        <div>
            <xsl:attribute name="id">divDetails<xsl:value-of select="$test.suite.id"/></xsl:attribute>
            <blockquote>
                <!-- The test classes, with failed tests grouped together at the top -->
                <xsl:apply-templates select=".//test-suite[@type='TestFixture'][results//test-case]">
                    <!-- Group by outcome, with failed tests first -->
                    <xsl:sort select="@success" order="ascending" data-type="text"/>
                    <!-- Then sort by name within the groups -->
                    <xsl:sort select="@name" order="ascending" data-type="text"/>
                </xsl:apply-templates>
            </blockquote>
        </div>
    </xsl:template>

    <xsl:template match="test-suite">
        <xsl:variable name="passedtests.list" select="results//test-case[@success='True']"/>
        <xsl:variable name="ignoredtests.list" select="results//test-case[@executed='False']"/>
        <xsl:variable name="failedtests.list" select="results//test-case[@success='False']"/>
        <xsl:variable name="tests.count" select="count(results//test-case)"/>
        <xsl:variable name="passedtests.count" select="count($passedtests.list)"/>
        <xsl:variable name="ignoredtests.count" select="count($ignoredtests.list)"/>
        <xsl:variable name="failedtests.count" select="count($failedtests.list)"/>

        <xsl:variable name="fixture.name">
            <xsl:call-template name="getFullFixtureName">
                <xsl:with-param name="node" select="."/>
            </xsl:call-template>
        </xsl:variable>
        <div>
            <div class="progress-bar">
                <xsl:call-template name="progressSegment">
                    <xsl:with-param name="value" select="$passedtests.count" />
                    <xsl:with-param name="total" select="$tests.count" />
                    <xsl:with-param name="class">passed-progress</xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="progressSegment">
                    <xsl:with-param name="value" select="$ignoredtests.count" />
                    <xsl:with-param name="total" select="$tests.count" />
                    <xsl:with-param name="class">ignored-progress</xsl:with-param>
                </xsl:call-template>

                <xsl:call-template name="progressSegment">
                    <xsl:with-param name="value" select="$failedtests.count" />
                    <xsl:with-param name="total" select="$tests.count" />
                    <xsl:with-param name="class">failed-progress</xsl:with-param>
                </xsl:call-template>
            </div>
            <div class="plus">
                <xsl:attribute name="id">imgTestCase_<xsl:value-of select="@name"/></xsl:attribute>
                <xsl:attribute name="onClick">
                    <xsl:text>javascript:toggleDiv('imgTestCase_</xsl:text>
                    <xsl:value-of select="$fixture.name"/>
                    <xsl:text>', 'divTest_</xsl:text>
                    <xsl:value-of select="$fixture.name"/>
                    <xsl:text>');</xsl:text>
                </xsl:attribute>
            </div>
            <a>
                <xsl:attribute name="name"><xsl:value-of select="$fixture.name"/></xsl:attribute>
                <xsl:value-of select="$fixture.name"/>
            </a>
            (<xsl:value-of select="$passedtests.count"/>/<xsl:value-of select="$tests.count"/>)

            <div style="display:none">
                <xsl:attribute name="id">divTest_<xsl:value-of select="$fixture.name"/></xsl:attribute>

                <xsl:apply-templates select="$failedtests.list"/>
                <xsl:apply-templates select="$ignoredtests.list"/>
                <xsl:apply-templates select="$passedtests.list"/>
            </div>
        </div>
    </xsl:template>

    <xsl:template match="test-case">
        <div>
            <xsl:if test="position() mod 2 = 0">
                <xsl:attribute name="class">section-oddrow</xsl:attribute>
            </xsl:if>

            <xsl:choose>
                <xsl:when test="./@executed='False'">
                    <div class="ignored" />
                </xsl:when>
                <xsl:when test="./@success='False'">
                    <div class="error" />
                </xsl:when>
                <xsl:otherwise>
                    <div class="check" />
                </xsl:otherwise>
            </xsl:choose>

            <span class="test-name">
                <xsl:call-template name="getTestName">
                    <xsl:with-param name="node" select="."/>
                </xsl:call-template>
            </span>

            <xsl:choose>
                <xsl:when test="./@executed='False'">
                    <xsl:value-of select="substring-after(reason/message, '-')"/>
                </xsl:when>
                <xsl:when test="./@success='False'">
                    <div class="failure-info">
                        <div class="failure-message">
                            <xsl:value-of select="failure/message"/>
                        </div>
                        <pre class="stacktrace">
                            <xsl:value-of select="failure/stack-trace"/>
                        </pre>
                    </div>
                </xsl:when>
            </xsl:choose>
        </div>
    </xsl:template>
    <xsl:template name="getFullFixtureName">
        <xsl:param name="node" />

        <xsl:for-each select="ancestor::test-suite[@type='TestFixture']/@name">
            <xsl:value-of select="." />
            <xsl:text>.</xsl:text>
        </xsl:for-each>
        <xsl:value-of select="@name" />
    </xsl:template>

    <xsl:template name="getTestName">
        <xsl:param name="node" />

        <!-- Concatenate ancestor namespaces into: One.Two.Three.  -->
        <xsl:variable name="namespace">
            <xsl:for-each select="ancestor::test-suite[@type='Namespace']/@name">
                <xsl:value-of select="." />
                <xsl:text>.</xsl:text>
            </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="fixture.name">
            <xsl:value-of select="ancestor::test-suite[@type='TestFixture']/@name" />
        </xsl:variable>

        <!-- Remove the namespace and fixture name from the test name -->
        <xsl:value-of select="substring(@name, string-length($namespace) + 1 + string-length($fixture.name) + 1)" />
    </xsl:template>

    <xsl:template name="progressSegment">
        <xsl:param name="value" />
        <xsl:param name="total" />
        <xsl:param name="class" />

        <xsl:variable name="percent" select="($value * 100) div $total" />

        <xsl:if test="$value > 0">
            <div class="{$class}">
                <xsl:attribute name="style">width: <xsl:value-of select="floor($percent * 100) div 100"/>%</xsl:attribute>
                &#160;
            </div>
        </xsl:if>
    </xsl:template>
</xsl:stylesheet>
