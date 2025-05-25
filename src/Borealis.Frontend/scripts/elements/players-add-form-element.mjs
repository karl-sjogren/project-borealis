import { LitElement, html, css } from 'lit';
import { Popover } from 'bootstrap';

export class PlayersAddForm extends LitElement {
  static get properties() {
    return {
      playerIdsString: { type: String, state: true },
      addAsInAlliance: { type: Boolean, state: true },
      showTable: { type: Boolean, state: true },
      loading: { type: Boolean, state: true },
      playerIdsError: { type: Boolean, state: true },
      players: { type: Array, state: true }
    };
  }

  createRenderRoot() {
    // Use the light DOM instead of shadow DOM
    return this;
  }


  constructor() {
    super();

    this.playerIdsString = '33820023, 123';
    this.addAsInAlliance = false;
  }

  updated() {
  }

  render() {
    if(!this.showTable) {
      return html`
        <div class="row">
          <form @submit=${this.formSubmit}>
            <div class="mb-3">
              <label for="playerIds" class="form-label">Player IDs</label>
              <textarea class="form-control" id="playerIds" name="PlayerIds" rows="6" .value=${this.playerIdsString} @input=${e => this.playerIdsString = e.target.value }></textarea>
            </div>

            <div class="form-check">
              <input type="checkbox" class="form-check-input" id="add-as-is-in-alliance" name="AddAsInAlliance" value="true" ?checked=${this.addAsInAlliance} @change=${e => this.addAsInAlliance = e.target.checked}>
              <label class="form-check-labe" for="add-as-is-in-alliance">
                Add as "In alliance"
              </label>
            </div>

            <button type="submit" class="btn btn-primary">Add players</button>
          </form>
        </div>
      `;
    }

    return html`
      ${!this.loading ? html`<button class="btn btn-primary btn-sm mb-3" @click=${this.resetClick}>Add more</button> ` : ''}


      <div class="table-responsive">
        <table class="table table-striped table-hover players-table">
          <thead>
            <tr>
              <th class="player-id">Player ID</th>
              <th class="furnace-level">Level</th>
              <th class="state">State</th>
              <th class="in-alliance">In alliance</th>
              <th class="name">Name</th>
              <th class="add-state">Status</th>
            </tr>
          </thead>
          <tbody>
            ${this.players.map(player => {
              if(player.addState === 'ERROR') {
                return html`
                  <tr class="player-row error">
                    <td>${player.externalId}</td>
                    <td colspan="5" class="text-danger">Failed to add player: ${player.errorMessage}</td>
                  </tr>
                `;
              }

              let icon = '';
              if(player.addState === 'LOADING') {
                icon = html`<i class="bi bi-hourglass-split"></i>`;
              } else if(player.addState === 'DONE') {
                icon = html`<i class="bi bi-check-circle-fill text-success"></i>`;
              } else if(player.addState === 'WAITING') {
                icon = html`<i class="bi bi-clock-fill text-secondary"></i>`;
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
                  </td>
                  <td>
                    ${icon}
                  </td>
                </tr>
              `
            })}
          </tbody>
        </table>
      </div>
    `;
  }

  parsePlayerIds() {
    if(!this.playerIdsString) {
      return [];
    }

    const parsedValues = this.playerIdsString
      .split(/[\s,]+/)
      .map(id => id.trim())
      .filter(id => id.length > 0)
      .map(id => parseInt(id, 10));

    const uniqueValues = new Set(parsedValues);
    const playerIds = Array.from(uniqueValues).filter(id => !isNaN(id));

    return playerIds;
  }

  /**
   * Adds or remove a player from the alliance
   * @param {SubmitEvent} event The form submit event
   * @returns {void>}
   */
  async formSubmit(event) {
    event.preventDefault();
    event.stopPropagation();

    const playerIds = this.parsePlayerIds();
    if(playerIds.length === 0) {
      this.playerIdsError = true;
      return;
    }

    this.players = playerIds.map(externalId => ({
      externalId: externalId,
      name: `Player ${externalId}`,
      addState: 'WAITING'
    }));

    this.showTable = true;

    for(const player of this.players) {
      player.addState = 'LOADING';
      this.players = [...this.players];

      const result = await this.addPlayer(player.externalId, this.addAsInAlliance);

      if(!result.success) {
        player.addState = 'ERROR';
        player.errorMessage = result.message;
        this.players = [...this.players];
        continue;
      }

      player.addState = 'DONE';
      player.name = result.data.name;
      player.isInAlliance = this.addAsInAlliance;
      player.furnaceLevelString = result.data.furnaceLevelString;
      player.state = result.data.state;

      this.players = [...this.players];
    }
  }

  resetClick() {
    this.playerIdsString = '';
    this.addAsInAlliance = false;
    this.showTable = false;
    this.loading = false;
    this.playerIdsError = false;
    this.players = [];
  }

  /**
   * Adds or remove a player from the alliance
   * @param {string} playerId The player ID
   * @param {boolean} addAsInAlliance True to add to alliance, false to remove from alliance
   * @returns {Promise<void>}
   */
  async addPlayer(playerId, addAsInAlliance) {
    const url = `/api/players/`;

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        playerId: playerId,
        addAsInAlliance: addAsInAlliance
      })
    });

    if(!response.ok) {
      console.error(`Failed to add plaeyer with id ${playerId}: ${response.status} ${response.statusText}`);
    }

    return response.json();
  };
}

window.customElements.define('players-add-form', PlayersAddForm);
