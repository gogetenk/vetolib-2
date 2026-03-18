var fs=require("fs");var lines=[];lines.push("# QA Report");fs.writeFileSync("e2e/reports/qa-portal-booking.md",lines.join("
"),"utf8");console.log("ok");