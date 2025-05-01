import { LitElement, html, css } from 'lit';
import { Popover } from 'bootstrap';

export class DiscordSettingsForm extends LitElement {
  static get properties() {
    return {
      loading: { type: Boolean },
      guilds: { type: Array },
      channels: { type: Array },
      selectedGuildId: { type: String },
      selectedChannelId: { type: String }
    };
  }

  createRenderRoot() {
    // Use the light DOM instead of shadow DOM
    return this;
  }

  constructor() {
    super();
    this.loading = true;
    this.guilds = [];
    this.channels = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this.loadGuilds();
  }

  render() {
    if(this.loading) {
      return html`<div class="alert alert-info">Loading stuff</div>`;
    }

    return html`
      <form @submit=${this.handleSubmit}>
        <div class="mb-3">
          <label for="guildId" class="form-label">Guild</label>
          <select class="form-select" id="guildId" name="guildId" @input=${this.guildChanged}>
            <option value="" selected @input=${this.guildChanged}>Select a guild</option>
            ${this.guilds.map(guild => html`
              <option value="${guild.guildId}" ?selected="${guild.guildId === this.selectedGuildId}">${guild.name}</option>
            `)}
          </select>
        </div>

        <div class="mb-3">
          <label for="giftCodeChannelId" class="form-label">Gift code channel</label>
          <select class="form-select" id="giftCodeChannelId" name="giftCodeChannelId" @input=${this.channelChanged}>
            <option value="" selected>Select a channel</option>
            ${this.channels.map(channel => html`
              <option value="${channel.channelId}" ?selected="${channel.channelId === this.selectedGiftCodeChannelId}">${channel.name}</option>
            `)}
          </select>
        </div>

        <div class="mb-3">
          <label for="playerRenameChannelId" class="form-label">Player rename channel</label>
          <select class="form-select" id="playerRenameChannelId" name="playerRenameChannelId" @input=${this.channelChanged}>
            <option value="" selected>Select a channel</option>
            ${this.channels.map(channel => html`
              <option value="${channel.channelId}" ?selected="${channel.channelId === this.selectedPlayerRenamedChannelId}">${channel.name}</option>
            `)}
          </select>
        </div>

        <div class="mb-3">
          <label for="playerFurnaceLevelChannelId" class="form-label">Player furnace level channel</label>
          <select class="form-select" id="playerFurnaceLevelChannelId" name="playerFurnaceLevelChannelId" @input=${this.channelChanged}>
            <option value="" selected>Select a channel</option>
            ${this.channels.map(channel => html`
              <option value="${channel.channelId}" ?selected="${channel.channelId === this.selectedPlayerFurnaceLevelChannelId}">${channel.name}</option>
            `)}
          </select>
        </div>

        <div class="mb-3">
          <label for="playerMovedStateChannelId" class="form-label">Player moved state channel</label>
          <select class="form-select" id="playerMovedStateChannelId" name="playerMovedStateChannelId" @input=${this.channelChanged}>
            <option value="" selected>Select a channel</option>
            ${this.channels.map(channel => html`
              <option value="${channel.channelId}" ?selected="${channel.channelId === this.selectedPlayerMoveStateChannelId}">${channel.name}</option>
            `)}
          </select>
        </div>

        <button type="submit" class="btn btn-primary">Save</button>
      </form>
    `;
  }

  async handleSubmit(event) {
    event.preventDefault();
    const formData = new FormData(event.target);
    const data = Object.fromEntries(formData.entries());

    console.log(data);
  }

  async guildChanged(event) {
    const selectedGuildId = event.target.value;
    this.selectedGuildId = selectedGuildId;
    //this.selectedChannelId = null;

    if (selectedGuildId) {
      await this.loadChannels(selectedGuildId);
    } else {
      this.channels = [];
    }
  }

  async loadGuilds() {
    this.loading = true;

    try {
      const response = await fetch('/api/discord/guilds');
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      this.guilds = await response.json();
    } catch (error) {
      console.error('Error fetching guilds:', error);
    } finally {
      this.loading = false;
    }
  }

  async loadChannels(guildId) {
    this.loading = true;

    try {
      const response = await fetch(`/api/discord/guilds/${guildId}/channels`);
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      this.channels = await response.json();
    } catch (error) {
      console.error('Error fetching channels:', error);
    } finally {
      this.loading = false;
    }
  }
}

window.customElements.define('discord-settings-form', DiscordSettingsForm);
