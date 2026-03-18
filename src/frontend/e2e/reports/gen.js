var fs=require("fs");var l=[];l.push("# QA Report");l.push("");l.push("status: QA_FAIL");fs.writeFileSync("e2e/reports/qa-portal-booking.md",l.join("
"),"utf8");console.log("ok");