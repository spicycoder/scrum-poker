import type { Meta, StoryObj } from '@storybook/react';
import { MemoryRouter } from 'react-router-dom';
import CreateRoomForm from './CreateRoomForm';

const meta: Meta<typeof CreateRoomForm> = {
  title: 'Tabs/Create',
  component: CreateRoomForm,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <MemoryRouter>
        <div className="w-full max-w-md mx-auto mt-8">
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof CreateRoomForm>;

export const Default: Story = {};
