import { LitElement, html, css } from 'lit';
import { Popover } from 'bootstrap';

export class PlayersTable extends LitElement {
  static get properties() {
    return {
      /**
       * The players data array as JSON
       * @type {Array}
       */
      players: { type: Array  }
    };
  }

  createRenderRoot() {
    // Use the light DOM instead of shadow DOM
    return this;
  }


  constructor() {
    super();
  }

  updated() {
    const popoverTriggerList = this.children[0].querySelectorAll('[data-bs-toggle="popover"]');
    [...popoverTriggerList].map(popoverTriggerEl => new Popover(popoverTriggerEl));
  }

  render() {
    return html`
      <div class="table-responsive">
        <table class="table table-striped table-hover players-table">
          <thead>
            <tr>
              <th class="player-id">Player ID</th>
              <th class="furnace-level">Level</th>
              <th class="state">State</th>
              <th class="in-alliance">In alliance</th>
              <th class="name">Name</th>
              <th class="actions"></th>
            </tr>
          </thead>
          <tbody>
            ${this.players.map(player => {
              let noteIcon = null;
              if(player.hasNotes) {
                noteIcon = html`
                  <button type="button" class="btn" data-bs-toggle="popover" data-bs-title="Notes" data-bs-content="${player.htmlNotes}" data-bs-html="true">
                    <span class="visually-hidden">Click to show notes</span>
                    <i class="bi bi-file-text-fill"></i>
                  </button>
                `;
              }

              let awayIcon = null;
              if(!!player.awayUntil) {
                awayIcon = html`
                  <button type="button" class="btn" data-bs-toggle="popover" data-bs-title="Player is away" data-bs-content="Player is away until ${player.awayUntil}">
                    <span class="visually-hidden">Player is away until ${player.awayUntil}</span>
                    <i class="bi bi-person-walking"></i>
                  </button>
                `;
              }

              let forceRedeemIcon = null;
              if(!player.isInAlliance && player.forceRedeemGiftCodes) {
                forceRedeemIcon = html`
                  <button type="button" class="btn" data-bs-toggle="popover" data-bs-title="Force redeem gift codes" data-bs-content="Force redeeming gift codes for this player even though they are not in an alliance.">
                    <span class="visually-hidden">Force redeeming gift codes for this player even though they are not in an alliance.</span>
                    <i class="bi bi-box2-heart"></i>
                  </button>
                `;
              }

              let mutedIcon = null;
              if(player.isMuted) {
                mutedIcon = html`
                  <button type="button" class="btn" data-bs-toggle="popover" data-bs-title="Player is muted" data-bs-content="Player is muted and won't have updated posted to discord">
                    <span class="visually-hidden">Player is muted and won't have updated posted to discord</span>
                    <i class="bi bi-volume-mute"></i>
                  </button>
                `;
              }

              return html`
                <tr class="player-row">
                  <td>${player.externalId}</td>
                  <td>
                    <span class="furnace-level level-${player.furnaceLevelString?.toLowerCase()}">
                      <span class="level-text">${player.furnaceLevelString}</span>
                    </span>
                  </td>
                  <td>${player.state}</td>
                  <td class="in-alliance">${player.isInAlliance ? "Yes" : "No"}</td>
                  <td>
                    ${player.name}
                    ${noteIcon}
                    ${awayIcon}
                    ${mutedIcon}
                    ${forceRedeemIcon}
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm" role="group" aria-label="Actions for player ${player.name}">
                      <a class="btn btn-primary" href="/players/${player.id}"><i class="bi bi-pencil"></i></a>

                      <div class="btn-group" role="group">
                        <button type="button" class="btn btn-primary dropdown-toggle" data-bs-toggle="dropdown" data-bs-auto-close="true" aria-expanded="false"></button>
                        <ul class="dropdown-menu">
                          ${player.isInAlliance ?
                            html`<li><button type="button" class="dropdown-item" @click=${e => this.updateInAllianceClick(player, false)}><i class="bi bi-person-dash"></i> Remove from alliance</button></li>` :
                            html`<li><button type="button" class="dropdown-item" @click=${e => this.updateInAllianceClick(player, true)}><i class="bi bi-person-add"></i> Add to alliance</button></li>`}

                          ${player.isMuted ?
                            html`<li><button type="button" class="dropdown-item" @click=${e => this.updateMuteClick(player, false)}><i class="bi bi-volume-up"></i> Unmute</button></li>` :
                            html`<li><button type="button" class="dropdown-item" @click=${e => this.updateMuteClick(player, true)}><i class="bi bi-volume-mute"></i> Mute</button></li>`}

                          ${!player.isInAlliance ?
                            player.forceRedeemGiftCodes ?
                              html`<li><button type="button" class="dropdown-item" @click=${e => this.updateForceRedeemClick(player, false)}><i class="bi bi-box2"></i> Don't force redeem gift codes</button></li>` :
                              html`<li><button type="button" class="dropdown-item" @click=${e => this.updateForceRedeemClick(player, true)}><i class="bi bi-box2-heart"></i> Force redeem gift codes</button></li>`
                            : null
                          }

                          <li><button class="dropdown-item" @click=${e => this.handleDeleteClick(player)}><i class="bi bi-trash"></i> Delete</button></li>
                        </ul>
                      </div>
                    </div>
                  </td>
                </tr>
              `
            })}
          </tbody>
        </table>
      </div>
    `;
  }

  async updateInAllianceClick(player, isInAlliance) {
    await this.updateAllianceStatus(player.id, isInAlliance);

    player.isInAlliance = isInAlliance;
    this.players = [...this.players];
  }

  async updateMuteClick(player, isMuted) {
    await this.updateMuteSetting(player.id, isMuted);

    player.isMuted = isMuted;
    this.players = [...this.players];
  }

  async updateForceRedeemClick(player, forceRedeem) {
    await this.updateForceRedeemSetting(player.id, forceRedeem);

    player.forceRedeemGiftCodes = forceRedeem;
    this.players = [...this.players];
  }

  async handleDeleteClick(player) {
    if(!confirm(`Are you sure you want to delete ${player.name}?`)) {
      return;
    }

    await this.deletePlayer(player.id);

    this.players = this.players.filter(p => p.id !== player.id);
  }

  /**
   * Adds or remove a player from the alliance
   * @param {string} playerId The player ID
   * @param {boolean} addToAlliance True to add to alliance, false to remove from alliance
   * @returns {Promise<void>}
   */
  async updateAllianceStatus(playerId, addToAlliance) {
    const url = `/api/players/${playerId}/${addToAlliance ? 'add-to-alliance' : 'remove-from-alliance'}`;

    const response = await fetch(url, { method: 'PUT' });

    if(!response.ok) {
      console.error(`Failed to change alliance status of player: ${response.status} ${response.statusText}`);
    }
  };

  /**
   *
   * @param {string} playerId The player ID
   * @param {boolean} mute True to mute, false to unmute
   * @returns {Promise<void>}
   */
  async updateMuteSetting(playerId, mute) {
    const url = `/api/players/${playerId}/${mute ? 'mute' : 'unmute'}`;

    const response = await fetch(url, { method: 'PUT' });

    if(!response.ok) {
      console.error(`Failed to update mute setting of player: ${response.status} ${response.statusText}`);
    }
  };

  /**
   * Updates the force redeem setting for a player
   * @param {string} playerId The player ID
   * @param {boolean} forceRedeem True to force redeem gift codes, false to not force redeem
   * @returns {Promise<void>}
   */
  async updateForceRedeemSetting(playerId, forceRedeem) {
    const url = `/api/players/${playerId}/${forceRedeem ? 'set-force-redeem-gift-codes' : 'unset-force-redeem-gift-codes'}`;

    const response = await fetch(url, { method: 'PUT' });

    if(!response.ok) {
      console.error(`Failed to update force redeem setting of player: ${response.status} ${response.statusText}`);
    }
  }

  /**
   * Removes a player from the system
   * @param {string} playerId The player ID
   * @returns {Promise<void>}
   */
  async deletePlayer(playerId) {
    const url = `/api/players/${playerId}`;

    const response = await fetch(url, { method: 'DELETE' });

    if(!response.ok) {
      console.error(`Failed to delete player: ${response.status} ${response.statusText}`);
    }
  };
}

window.customElements.define('players-table', PlayersTable);
