if (!window.console) {
    var console = {
        log: function () { },
        warn: function () { },
        error: function () { },
        time: function () { },
        timeEnd: function () { }
    }
}

function TSUpdateLog(period) {
    var xmlhttp = new XMLHttpRequest();
    var logTable = document.getElementById("LogTable");
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) {
                var traces = JSON.parse(xmlhttp.responseText);
                logTable.sessionId = traces.SessionId;
                if (traces.Records != null) {
                    for (var i = 0; i < traces.Records.length; i++) {
                        var newRow = logTable.insertRow(logTable.rows.length);

                        var record = traces.Records[i];
                        newRow.insertCell(0).appendChild(document.createTextNode(record.Index));
                        newRow.insertCell(1).appendChild(document.createTextNode(record.Ticks));
                        newRow.insertCell(2).appendChild(document.createTextNode(record.ProcessName));
                        newRow.insertCell(3).appendChild(document.createTextNode(record.Text));
                        newRow.insertCell(4).appendChild(document.createTextNode(''));
                        logTable.lastIndex = record.Index + 1;

                        newRow.scrollIntoView();
                    }
                }
                else {
                    if (traces.LostCount < 0) { // new session
                        logTable.lastIndex = 0;
                    }
                }
            }
            else if (xmlhttp.status == 0) {
                document.getElementById("ServerStatus").innerHTML = "<span style='color:red'>Socket error</span>";
            }
            else {
                document.getElementById("ServerStatus").innerHTML = "<span style='color:red'>" + xmlhttp.statusText + " (" + xmlhttp.status + ")</span>";
            }
        }
    }
    var lastIndex = logTable.lastIndex;
    if (typeof lastIndex == 'undefined') {
        lastIndex = 0;
    }
    var sessionId = logTable.sessionId;
    if (typeof sessionId == 'undefined') {
        sessionId = '';
    }
    xmlhttp.open("GET", "json/traces/" + lastIndex + "/" + sessionId, true);
    xmlhttp.send();
    window.setTimeout(function () { TSUpdateLog(period); }, period);
}

function TSUpdateDiag(period) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) {
                var diag = JSON.parse(xmlhttp.responseText);
                document.getElementById("ServerStatus").innerHTML = "<span style='color:green'>OK</span>";
                document.getElementById("ServerVersion").innerHTML = diag.Version + ' - Compiled ' + diag.AssemblyDate;
                document.getElementById("ServerTime").innerHTML = diag.Time;
                document.getElementById("OSVersion").innerHTML = diag.OSVersion;
                document.getElementById("ClrVersion").innerHTML = diag.ClrVersion;
                document.getElementById("CpuUsage").innerHTML = diag.CpuUsage;
                document.getElementById("ProcessorCount").innerHTML = diag.ProcessorCount;
                document.getElementById("MemoryUsage").innerHTML = diag.MemoryUsage + ' / ' + diag.WorkingSet;
                document.getElementById("BufferCapacity").innerHTML = diag.BufferCapacity;
                document.getElementById("BufferCount").innerHTML = diag.BufferCount;
                document.getElementById("BufferTotalCount").innerHTML = diag.BufferTotalCount;
                document.getElementById("SessionId").innerHTML = diag.SessionId;
                var confTable = document.getElementById("ConfTable");
                confTable.innerHTML = '';
                for (var i = 0; i < diag.Prefixes.length; i++) {
                    var newRow = confTable.insertRow(confTable.rows.length);
                    var newCell = newRow.insertCell(0);
                    var newText = document.createTextNode('Prefix[' + i + '] Enabled');
                    newCell.appendChild(newText);
                    newCell = newRow.insertCell(1);
                    newText = document.createTextNode(diag.Prefixes[i].Enabled);
                    newCell.appendChild(newText);

                    newRow = confTable.insertRow(confTable.rows.length);
                    newCell = newRow.insertCell(0);
                    newText = document.createTextNode('Prefix[' + i + '] Uri');
                    newCell.appendChild(newText);
                    newCell = newRow.insertCell(1);
                    newText = document.createTextNode(diag.Prefixes[i].Uri);
                    newCell.appendChild(newText);
                }
            }
            else if (xmlhttp.status == 0) {
                document.getElementById("ServerStatus").innerHTML = "<span style='color:red'>Socket error</span>";
            }
            else {
                document.getElementById("ServerStatus").innerHTML = "<span style='color:red'>" + xmlhttp.statusText + " (" + xmlhttp.status + ")</span>";
            }
        }
    }
    xmlhttp.open("GET", "json/diag", true);
    xmlhttp.send();
    window.setTimeout(function () { TSUpdateDiag(period); }, period);
}