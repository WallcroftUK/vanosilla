#
# Regular cron jobs for the vanosilla package
#
0 4	* * *	root	[ -x /usr/bin/vanosilla_maintenance ] && /usr/bin/vanosilla_maintenance
