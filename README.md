# Project Borealis

Alliance management tool for Whiteout Survival. Currently used by AFO in state
851 but could setup and used by any alliance.

## Features

* Log in with Discord to get access to the alliance management tool
* Add players by their player ID, name and furnace level is fetched automatically
  * Syncs players with the game every hour and keeps record of name changes
* Mark players as in alliance or not, players in alliance have gift codes automatically remdeemed
* Add and redeem gift codes for all alliance members. Also tracks which players a code has been redeemed for
  * Checks for new gift codes on external sites on a regular basis and auto redeems them
* Save message templates that all leaders might want to send to all members such as bear formations or alliance event sign ups

Project icon by [Freepik](https://www.flaticon.com/authors/freepik) from [Flaticon](https://www.flaticon.com/)

## How it works

When iPhone/iPad users get a gift code they can't redeem it in game and instead have to go to
<https://wos-giftcode.centurygame.com/>. There they enter their PlayerId which is then verified
by returning information such as the player name, furnace level and state via an HTTP endpoint.

Since this is a public thing that any one can access it can also be used to automatically retrieve
information about a specific PlayerId (and redeem gift codes for that player).
